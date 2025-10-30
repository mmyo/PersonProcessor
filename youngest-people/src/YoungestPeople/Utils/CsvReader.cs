using System;
using System.Collections.Generic;
using System.IO;
using YoungestPeople.Models;

namespace YoungestPeople.Utils
{
    /// <summary>
    /// Provides efficient streaming CSV reading functionality for Person records.
    /// </summary>
    public static class CsvReader
    {
        /// <summary>
        /// Checks if a line appears to be a CSV header based on common column naming patterns.
        /// </summary>
        private static bool IsHeaderLine(string line)
        {
            var low = line.ToLowerInvariant();
            return low.Contains("first") && low.Contains("last") && low.Contains("birth");
        }

        /// <summary>
        /// Reads and parses Person records from a CSV file, streaming one at a time to minimize memory usage.
        /// Automatically detects and skips header row if present. Invalid lines are skipped and optionally reported.
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="onInvalidLine">Optional callback for invalid lines (for logging/reporting)</param>
        /// <returns>An enumerable of valid Person objects from the CSV</returns>
        public static IEnumerable<Person> ReadPeople(string path, Action<string?>? onInvalidLine = null)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var sr = new StreamReader(fs);

            string? line;
            bool headerChecked = false;
            while ((line = sr.ReadLine()) != null)
            {
                if (!headerChecked)
                {
                    headerChecked = true;
                    if (IsHeaderLine(line))
                        continue;
                }

                if (Person.TryParseFromCsv(line, out var person))
                {
                    yield return person!;
                }
                else
                {
                    onInvalidLine?.Invoke(line);
                }
            }
        }
    }
}
