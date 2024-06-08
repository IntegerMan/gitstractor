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

        string modelPath = "/home/matteland/models/";

        // Get a list of available models in the model path
        string[] models = Directory.GetFiles(modelPath, "*.gguf");
        if (models.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]No models found in the specified path.[/]");
            return 1;
        }

        // Prompt the user to select a model if we have > 1 model
        string selectedModel;
        if (models.Length == 1)
        {
            selectedModel = models[0];
        }
        else
        {
            selectedModel = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Select a model")
                .AddChoices(models));
        }

        modelPath = Path.Combine(modelPath, selectedModel);

        AnsiConsole.MarkupLineInterpolated($"Setting up LLama model from [yellow]{modelPath}[/]");

        var parameters = new ModelParams(modelPath)
        {
            ContextSize = 2048, // The longest length of chat as memory.
            GpuLayerCount = int.MaxValue
        };
        using LLamaWeights model = LLamaWeights.LoadFromFile(parameters);
        using LLamaContext context = model.CreateContext(parameters);
        ILLamaExecutor executor = new StatelessExecutor(model, context.Params);

        // Add chat histories as prompt to tell AI how to act.
        InferenceParams inferenceParams = new()
        {
            MaxTokens = 128,
            AntiPrompts = new List<string> { "User:", "}", "Assistant:", "<|user|>", "<|end|>", "<|assistant|>" }
        };

        const string systemPrompt =
            "You are an AI agent built to classify git commits as being related to fixing bugs or not being related to fixing bugs." +
            "Consider the input JSON to determine if it represents a bugfix commit. Respond only with \"{\"IsBugFix\": boolean_value, \"Reason\": \"A few words of explanation\"}\" " +
            "where boolean_value must be either true or false and never null, empty, or undefined. " +
            "Only indicate if a commit is a bugfix commit if there is a high probability that the commit is intended to fix a known problem in the code. " +
            "Adding features does not count as a bug fix. " +
            "When providing a reason, keep the explanation short and to a single sentence." +
            "Some examples:";

        string[] examples =
        [
            """
            <|user|>
            "Fixed build issues"
            <|end|>

            <|assistant|>
            {"IsBugFix": true, "Reason": "Mentioned fixing things"}
            <|end|>
            """,            
            """
            <|user|>
            "Rendering user list and starting on internal logic"
            <|end|>

            <|assistant|>
            {"IsBugFix": false, "Reason": "Looks like progress on new features"}
            <|end|>
            """,
            """
            <|user|>
            "Refactoring code to be more testable"
            <|end|>

            <|assistant|>
            {"IsBugFix": false, "Reason": "Refactoring and testability are not bugfixes"}
            <|end|>
            """,

            """
            <|user|>
            "Adding logging"
            <|end|>

            <|assistant|>
            {"IsBugFix": false, "Reason": "Logging can help identify bugs, but does not represent a fix"}
            <|end|>
            """,

            """
            <|user|>
            "For bob"
            <|end|>

            <|assistant|>
            {"IsBugFix": false, "Reason": "The message is ambiguous so defaulting to false"}
            <|end|>
            """,

            """
            <|user|>
            "Starting dark theme"
            <|end|>

            <|assistant|>
            {"IsBugFix": false, "Reason": "This looks like a new feature, not a bugfix."}
            <|end|>
            """
        ];
        string prompts = systemPrompt + "\n" +
                         string.Join("\n", examples) + "\r" +
                         "Actual scenario to evaluate follows: + \n\n";
        
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
                string json = JsonConvert.SerializeObject(row.Message);
                StringBuilder sb = new(prompts);
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

                // Remove content before the first { and after the last }
                int start = resp.IndexOf('{');
                int end = resp.LastIndexOf('}');
                if (start >= 0 && end >= 0)
                {
                    resp = resp[start..(end + 1)];
                }

                // Parse the JSON
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
        AnsiConsole.MarkupLineInterpolated(
            $"[cyan]Elapsed time:[/] {elapsed.TotalMilliseconds / 1000.0:0.00} seconds ({(elapsed.TotalMilliseconds / (double)commits.Count) / 1000.0:0.00} seconds per commit)");

        return 0;
    }
}