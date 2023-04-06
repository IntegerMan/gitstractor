using System.Globalization;
using CsvHelper;

namespace GitStractor.Readers;

public static class FileCsvReader
{
    public static IEnumerable<FileData> ReadFileCommits(string csvFilePath)
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
