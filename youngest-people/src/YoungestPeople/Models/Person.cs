using System;

namespace YoungestPeople.Models
{
    public sealed class Person
    {
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime DateOfBirth { get; }

        public Person(string firstName, string lastName, DateTime dateOfBirth)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            DateOfBirth = dateOfBirth;
        }

        public override string ToString()
        {
            return $"{FirstName} {LastName} ({DateOfBirth:yyyy-MM-dd})";
        }

        /// <summary>
        /// Cleans up a CSV field by trimming whitespace and removing enclosing quotes.
        /// </summary>
        private static string CleanCsvField(string s)
        {
            s = s.Trim();
            if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
                return s.Substring(1, s.Length - 2);
            return s;
        }

        /// <summary>
        /// Attempts to parse a Person from a CSV line in the format: FirstName,LastName,DateOfBirth
        /// Handles quoted fields and whitespace. Date must be in a format parseable by DateTime.Parse.
        /// </summary>
        /// <param name="line">The CSV line to parse</param>
        /// <param name="person">The parsed Person object if successful, null otherwise</param>
        /// <returns>true if parsing was successful, false otherwise</returns>
        public static bool TryParseFromCsv(string line, out Person? person)
        {
            person = null;
            if (string.IsNullOrWhiteSpace(line)) return false;

            // Split into exactly 3 parts for performance (avoids parsing entire CSV line)
            var parts = line.Split(',', 3);
            if (parts.Length < 3) return false;

            var first = CleanCsvField(parts[0]);
            var last = CleanCsvField(parts[1]);
            var dobRaw = CleanCsvField(parts[2]);

            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last)) return false;

            if (!DateTime.TryParse(dobRaw, out var dob)) return false;

            person = new Person(first, last, dob.Date);
            return true;
        }
    }
}
