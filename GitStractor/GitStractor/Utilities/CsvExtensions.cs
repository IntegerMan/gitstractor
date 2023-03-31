using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitStractor.Utilities;

public static class CsvExtensions
{
    /// <summary>
    /// Takes a string and potentially encodes it for safety in a CSV file by conditionally adding quotes.
    /// This protects against commas embedded in values which could confuse the outputted file
    /// </summary>
    /// <param name="value">The string to transform</param>
    /// <returns>The CSV safe string</returns>
    public static string? ToCsvSafeString(this string? value) 
        => value != null && value.Contains(",")
            ? $"\"{value}\""
            : value;
}