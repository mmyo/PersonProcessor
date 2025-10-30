using System;
using System.IO;
using System.Text;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: TestDataGenerator <output.csv> <count>");
            return 1;
        }

        var path = args[0];
        if (!int.TryParse(args[1], out var count) || count <= 0)
        {
            Console.Error.WriteLine("Invalid count");
            return 2;
        }

        var rnd = new Random(42);
        var firstNames = new[] { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Garcia", "Rodriguez", "Wilson", "Martinez", "Anderson", "Taylor", "Thomas", "Hernandez", "Moore", "Martin", "Jackson", "Thompson", "White" };

        // Date range from 1900-01-01 to today
        var start = new DateTime(1900, 1, 1);
        var span = (DateTime.Today - start).Days;

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var sw = new StreamWriter(fs, Encoding.UTF8, 65536);

        // optional header
        sw.WriteLine("FirstName,LastName,DateOfBirth");

        var sb = new StringBuilder(64);
        for (int i = 0; i < count; i++)
        {
            var fn = firstNames[rnd.Next(firstNames.Length)];
            var ln = lastNames[rnd.Next(lastNames.Length)];
            var dob = start.AddDays(rnd.Next(span + 1));

            // quick CSV: no commas in names
            sb.Clear();
            sb.Append(fn).Append(',').Append(ln).Append(',').Append(dob.ToString("yyyy-MM-dd"));
            sw.WriteLine(sb.ToString());

            if ((i & 0x3FFFF) == 0 && i > 0) // every ~262k rows
            {
                Console.WriteLine($"Generated {i} rows...");
            }
        }

        Console.WriteLine($"Generated {count} rows to {path}");
        return 0;
    }
}
