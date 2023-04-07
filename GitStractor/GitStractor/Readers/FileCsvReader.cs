using System.Globalization;
using CsvHelper;

namespace GitStractor.Readers;

public static class FileCsvReader
{
    public static IEnumerable<FileCommitData> ReadFileCommits(string csvFilePath)
    {
        using (StreamReader reader = new(csvFilePath))
        {
            using (CsvReader csv = new(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<FileCommitData>().ToList();
            }
        }
    }

    public static IEnumerable<FileData> ReadFiles(string csvFilePath)
    {
        using (StreamReader reader = new(csvFilePath))
        {
            using (CsvReader csv = new(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<FileData>().ToList();
            }
        }
    }
}
