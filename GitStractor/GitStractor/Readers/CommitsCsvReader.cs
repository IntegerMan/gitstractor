using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitStractor.Readers;

public static class CommitsCsvReader
{
    public static IEnumerable<CommitData> ReadCommits(string csvFilePath)
    {
        using (StreamReader reader = new(csvFilePath))
        {
            using (CsvReader csv = new(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<CommitData>().ToList();
            }
        }
    }
}