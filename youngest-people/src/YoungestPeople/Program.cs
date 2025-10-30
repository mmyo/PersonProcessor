using System;
using System.Diagnostics;
using System.IO;
using YoungestPeople.Services;
using YoungestPeople.Utils;

class Program
{
    static int Main(string[] args)
    {
        const int DEFAULT_COUNT = 10;
        const int INVALID_REPORT_THRESHOLD = 1024;  // Log every N invalid lines

        Console.WriteLine("Youngest People Finder — Reads a CSV and finds the youngest people");
        Console.WriteLine("----------------------------------------------------------------");

        // Validate command line arguments
        if (args.Length == 0)
        {
            Console.WriteLine($"Usage: <path-to-csv> [count={DEFAULT_COUNT}]");
            Console.WriteLine("Example: people.csv 20");
            return 1;
        }

        var path = args[0];
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"Error: File not found: {path}");
            return 2;
        }

        // Parse the optional count parameter
        int take = DEFAULT_COUNT;
        if (args.Length > 1 && !int.TryParse(args[1], out take))
        {
            Console.Error.WriteLine($"Warning: Invalid count '{args[1]}', using default {DEFAULT_COUNT}.");
            take = DEFAULT_COUNT;
        }

        var processor = new PersonProcessor();

        // Track and report invalid lines periodically
        int invalid = 0;
        void OnInvalid(string? line)
        {
            invalid++;
            if ((invalid % INVALID_REPORT_THRESHOLD) == 0)
            {
                Console.Error.WriteLine($"Warning: {invalid} invalid lines encountered...");
            }
        }

        Console.WriteLine($"\nProcessing {path} to find the {take} youngest people...\n");

        var sw = Stopwatch.StartNew();
        try
        {
            // Stream and process the CSV
            var people = CsvReader.ReadPeople(path, OnInvalid);
            var top = processor.GetYoungest(people, take);
            sw.Stop();

            Console.WriteLine();
            Console.WriteLine($"Top {top.Count} youngest people (youngest first):\n");
            for (int i = 0; i < top.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {top[i]}");
            }

            Console.WriteLine();
            Console.WriteLine($"Processed in {sw.Elapsed.TotalSeconds:F2}s. Invalid lines: {invalid}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return 3;
        }
    }
}
