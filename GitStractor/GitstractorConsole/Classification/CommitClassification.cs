using System.Globalization;
using System.Text;
using CsvHelper;
using GitStractor.Readers;
using LLama;
using LLama.Abstractions;
using LLama.Common;
using LLama.Native;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Json;

namespace GitstractorConsole.Classification;

public class CommitClassification
{
    public async Task<int> RunAsync()
    {
        string directory = "/home/matteland/data/";
        
        // Get directories in that directory
        string[] directories = Directory.GetDirectories(directory);
        if (directories.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]No directories found in the specified path.[/]");
            return 1;
        }
        
        // Prompt the user to select a directory
        directory = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select a directory")
            .AddChoices(directories));
        
        string commitFile = Path.Combine(directory, "Commits.csv");
        string filePath = Path.Combine(directory, commitFile);

        AnsiConsole.MarkupLineInterpolated($"Reading commits from [bold yellow]{filePath}[/]");

        List<GitCommitRow> commits = CommitsCsvReader.ReadCommits(filePath).ToList();

        AnsiConsole.MarkupLineInterpolated($"Read [bold yellow]{commits.Count()}[/] commits");

        // Set up the LLama model
        NativeLibraryConfig.Instance.WithLogCallback(delegate(LLamaLogLevel level, string message)
        {
            //AnsiConsole.WriteLine($"{level}: {message}");
        });

        string modelPath = @"/home/matteland/models/";

        // Get a list of available models in the model path
        string[] models = Directory.GetFiles(modelPath, "*.gguf");
        if (models.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]No models found in the specified path.[/]");
            return 1;
        }

        // Prompt the user to select a model
        string selectedModel = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select a model")
            .AddChoices(models));

        modelPath = Path.Combine(modelPath, selectedModel);

        AnsiConsole.MarkupLineInterpolated($"Setting up LLama model from [yellow]{modelPath}[/]");

        var parameters = new ModelParams(modelPath)
        {
            ContextSize = 1024, // The longest length of chat as memory.
            GpuLayerCount = 8 // How many layers to offload to GPU. Please adjust it according to your GPU memory.
        };
        using LLamaWeights model = LLamaWeights.LoadFromFile(parameters);
        using LLamaContext context = model.CreateContext(parameters);
        ILLamaExecutor executor = new StatelessExecutor(model, context.Params);

        // Add chat histories as prompt to tell AI how to act.
        InferenceParams inferenceParams = new()
        {
            MaxTokens = 64,
            AntiPrompts = new List<string> { "User:", "}", "Assistant:", "<|user|>", "<|end|>", "<|assistant|>" }
        };

        const string systemPrompt =
            "Consider the input JSON to determine if it represents a bugfix commit. Respond only with \"{\"IsBugFix\": boolean_value, \"Reason\": \"A few words of explanation\"}\" Some examples:";

        const string example1 = """
                                <|user|>
                                {
                                   "Message": "Fixed build issues",
                                   "WorkItems": 0,
                                   "TotalFiles": 11,
                                   "ModifiedFiles": 4,
                                   "AddedFiles": 6,
                                   "DeletedFiles": 1,
                                   "TotalLines": 7346,
                                   "NetLines": 6904,
                                   "AddedLines": 6928,
                                   "DeletedLines": 24,
                                   "IsMerge": false
                                }
                                <|end|>

                                <|assistant|>
                                {"IsBugFix": false, "Reason": "Mentioned fixing things"}
                                <|end|>
                                """;

        const string example2 = """
                                <|user|>
                                {
                                   "Message": "Refactoring code to be more testable",
                                   "WorkItems": 0,
                                   "TotalFiles": 13,
                                   "ModifiedFiles": 5,
                                   "AddedFiles": 8,
                                   "DeletedFiles": 0,
                                   "TotalLines": 454,
                                   "NetLines": 165,
                                   "AddedLines": 210,
                                   "DeletedLines": 45,
                                   "IsMerge": false
                                }
                                <|end|>

                                <|assistant|>
                                {"IsBugFix": false, "Reason": "Refactoring and testability are not bugfixes"}
                                <|end|>
                                """;

        string outputPath = Path.Combine(directory, "ClassifiedCommits.csv");

        TimeSpan elapsed = TimeSpan.Zero;
        await AnsiConsole.Progress().StartAsync(async prog =>
        {
            ProgressTask task = prog.AddTask($"Classifying {commits.Count} commits...", autoStart: false);
            task.MaxValue = commits.Count;
            
            await using FileStream file = new(outputPath, FileMode.Create);
            await using CsvWriter writer = new CsvWriter(new StreamWriter(file), CultureInfo.CurrentCulture);
            writer.WriteField("Sha");
            writer.WriteField("Message");
            writer.WriteField("IsBugFix");
            writer.WriteField("Message");
            writer.WriteField("Response");
            await writer.NextRecordAsync();
            
            task.StartTask();
            
            foreach (var row in commits)
            {
                string json = JsonConvert.SerializeObject(row, Formatting.Indented);

                StringBuilder sb = new(systemPrompt);
                sb.AppendLine(example1);
                sb.AppendLine(example2);
                sb.AppendLine("Actual scenario to evaluate follows:");
                sb.AppendLine();
                sb.AppendLine("<|user|>");
                sb.AppendLine(json);
                sb.AppendLine("<|end|>");
                sb.AppendLine("<|assistant|>");

                StringBuilder response = new();
                await foreach (var text in executor.InferAsync(sb.ToString(), inferenceParams))
                {
                    response.Append(text);
                }

                string resp = response.ToString()
                    .ReplaceLineEndings(" ")
                    .Trim();

                ClassificationResult? classify = null;

                try
                {
                    classify = JsonConvert.DeserializeObject<ClassificationResult>(resp);
                }
                catch (JsonException)
                {
                    // Ignore - we'll keep our variable null and log it out for diagnostics
                }

                writer.WriteField(row.Sha);
                writer.WriteField(row.Message, shouldQuote: true);
                writer.WriteField(classify?.IsBugFix);
                writer.WriteField(classify?.Reason ?? resp, shouldQuote: true);
                writer.WriteField(resp);
                await writer.NextRecordAsync();

                task.Value++;
            }

            await writer.FlushAsync();
            task.StopTask();
            
            elapsed = task.ElapsedTime!.Value;
        });

        AnsiConsole.MarkupLineInterpolated($"[Green]Wrote classified commits to[/] [yellow]{outputPath}[/]");
        AnsiConsole.MarkupLineInterpolated($"[cyan]Elapsed time:[/] {elapsed.TotalMilliseconds/1000.0:0.00} seconds ({(elapsed.TotalMilliseconds / (double)commits.Count)/1000.0:0.00} seconds per commit)");

        return 0;
    }
}