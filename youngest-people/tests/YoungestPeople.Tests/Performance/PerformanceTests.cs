using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using YoungestPeople.Services;
using YoungestPeople.Utils;

namespace YoungestPeople.Tests.Performance
{
    public class PerformanceTests
    {
        private readonly ITestOutputHelper _output;

        public PerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private string GenerateTestData(int count)
        {
            var path = Path.Combine(Path.GetTempPath(), $"perf-test-{count}.csv");
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
            // Get the path to the solution root
            var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../youngest-people/"));
            _output.WriteLine($"Solution root: {solutionRoot}");
            
            // Get the path to the test data generator
            var generatorProject = Path.Combine(solutionRoot, "tools", "TestDataGenerator");
            _output.WriteLine($"Generator project: {generatorProject}");
            
            // Verify the directory exists
            if (!Directory.Exists(generatorProject))
            {
                _output.WriteLine("Generator project directory not found!");
                var files = Directory.GetFiles(solutionRoot, "*.csproj", SearchOption.AllDirectories);
                _output.WriteLine("Found .csproj files:");
                foreach (var file in files)
                {
                    _output.WriteLine(file);
                }
                throw new DirectoryNotFoundException($"Could not find {generatorProject}");
            }
            
            _output.WriteLine($"Using test data generator at: {generatorProject}");

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = generatorProject,
                Arguments = $"run --no-build -- {path} {count}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(startInfo);
            var output = process!.StandardOutput.ReadToEnd();
            var error = process!.StandardError.ReadToEnd();
            process.WaitForExit();

            _output.WriteLine($"TestDataGenerator output: {output}");
            if (!string.IsNullOrEmpty(error))
            {
                _output.WriteLine($"TestDataGenerator error: {error}");
            }

            Assert.Equal(0, process.ExitCode);
            return path;
        }

        private (TimeSpan elapsed, long memoryBytes, int invalidLines) MeasureProcessing(string path, int take = 10)
        {
            // Collect garbage to get a clean memory measurement
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var startMemory = GC.GetTotalMemory(true);
            var sw = Stopwatch.StartNew();

            int invalid = 0;
            var processor = new PersonProcessor();
            var people = CsvReader.ReadPeople(path, line => invalid++);
            var youngest = processor.GetYoungest(people, take);

            sw.Stop();
            var endMemory = GC.GetTotalMemory(false);

            // Verify we got results (sanity check)
            Assert.NotEmpty(youngest);
            Assert.True(youngest.Count <= take);

            return (sw.Elapsed, endMemory - startMemory, invalid);
        }

        [Theory]
        [InlineData(1_000)]
        [InlineData(10_000)]
        [InlineData(100_000)]
        public void MeasureScaling(int recordCount)
        {
            var path = GenerateTestData(recordCount);
            try
            {
                var (elapsed, memoryBytes, invalidLines) = MeasureProcessing(path);

                var fileInfo = new FileInfo(path);
                var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);
                var recordsPerSecond = recordCount / elapsed.TotalSeconds;
                var mbPerSecond = fileSizeMB / elapsed.TotalSeconds;
                var memoryMB = memoryBytes / (1024.0 * 1024.0);

                _output.WriteLine($"\nPerformance with {recordCount:N0} records:");
                _output.WriteLine($"File size: {fileSizeMB:F2} MB");
                _output.WriteLine($"Time: {elapsed.TotalSeconds:F2} seconds");
                _output.WriteLine($"Throughput: {recordsPerSecond:N0} records/second");
                _output.WriteLine($"           {mbPerSecond:F2} MB/second");
                _output.WriteLine($"Memory: {memoryMB:F2} MB");
                _output.WriteLine($"Invalid lines: {invalidLines}");
            }
            finally
            {
                try { File.Delete(path); } catch { }
            }
        }

        [Fact]
        public void TestLargeFile()
        {
            const int recordCount = 1_000_000;
            var path = GenerateTestData(recordCount);
            try
            {
                var (elapsed, memoryBytes, invalidLines) = MeasureProcessing(path);

                var fileInfo = new FileInfo(path);
                var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);
                var recordsPerSecond = recordCount / elapsed.TotalSeconds;
                var mbPerSecond = fileSizeMB / elapsed.TotalSeconds;
                var memoryMB = memoryBytes / (1024.0 * 1024.0);

                _output.WriteLine($"\nLarge file performance ({recordCount:N0} records):");
                _output.WriteLine($"File size: {fileSizeMB:F2} MB");
                _output.WriteLine($"Time: {elapsed.TotalSeconds:F2} seconds");
                _output.WriteLine($"Throughput: {recordsPerSecond:N0} records/second");
                _output.WriteLine($"           {mbPerSecond:F2} MB/second");
                _output.WriteLine($"Memory: {memoryMB:F2} MB");
                _output.WriteLine($"Invalid lines: {invalidLines}");

                // Basic performance assertions (adjust these based on your requirements)
                Assert.True(memoryMB < 100, "Memory usage should be under 100MB");
                Assert.True(recordsPerSecond > 10_000, "Should process at least 10K records/second");
            }
            finally
            {
                try { File.Delete(path); } catch { }
            }
        }
    }
}