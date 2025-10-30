# Youngest People

A simple .NET console app that reads a CSV of people (FirstName,LastName,DateOfBirth) and prints the N youngest people.

## Quick Start

Build the solution:
```powershell
dotnet build
```

Generate 1,000,000 records:
```powershell
dotnet run --project ./youngest-people/tools/TestDataGenerator/TestDataGenerator.csproj -- sample.csv 1000000
```

Find the 10 youngest people in the CSV:
```powershell
dotnet run --project ./youngest-people/src/YoungestPeople/YoungestPeople.csproj -- sample.csv 10
```

Run all tests:
```powershell
dotnet test
```

## Solution Structure

- `src\YoungestPeople` - Main console application
- `tools\TestDataGenerator` - Tool to generate sample CSV data
- `tests\YoungestPeople.Tests` - Unit tests

## Command Details

### Generate Test Data
```powershell
dotnet run --project .\youngest-people\tools\TestDataGenerator\TestDataGenerator.csproj -- <output.csv> <count>
```

Creates a CSV with header and the specified number of random person records.

### Process CSV
```powershell
dotnet run --project .\youngest-people\src\YoungestPeople\YoungestPeople.csproj -- <input.csv> [count=10]
```

Reads the CSV and prints the N youngest people (default 10).

## Design & Implementation Notes

The solution prioritizes performance and memory efficiency:

- **Memory Efficient**: Streams the CSV file line-by-line to maintain constant memory usage regardless of file size
- **Optimal Algorithm**: Uses a fixed-size min-heap (PriorityQueue) to maintain top N youngest people
  - Time complexity: O(n log k) where n = number of records, k = number taken
  - Space complexity: O(k) where k = number taken
- **Robust Processing**: 
  - Automatically detects and skips CSV headers
  - Handles invalid lines gracefully with optional reporting
  - Supports quoted CSV fields
  - Memory-efficient string handling

## Performance Results

Testing on a modern desktop machine (Windows 10, Ryzen 7):

| Input Size  | Processing Time | Throughput     | Memory Usage |
|-------------|----------------|----------------|--------------|
| 1,000       | <0.01s        | ~580k rec/sec  | 0.29 MB     |
| 10,000      | 0.01s         | ~708k rec/sec  | 2.74 MB     |
| 100,000     | 0.10s         | ~982k rec/sec  | 3.28 MB     |
| 1,000,000   | 0.48s         | ~2.1M rec/sec  | 2.38 MB     |

Key observations:
- Memory usage remains low and stable even with large datasets
- Performance scales well, showing better throughput with larger files
- Zero invalid lines in test data, robust parsing

## Assumptions & Limitations

- CSV parsing is intentionally simple (splits into three fields). It handles quoted values without embedded commas. For production, consider a robust CSV parser like CsvHelper.
- Date parsing uses `DateTime.TryParse` which accepts common date formats. For strict formats, update the parser to use `TryParseExact`.
