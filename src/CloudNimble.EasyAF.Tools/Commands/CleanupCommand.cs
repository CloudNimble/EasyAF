using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Command for cleaning up build artifacts and lock files from the solution.
    /// </summary>
    /// <remarks>
    /// This command recursively deletes bin, obj, TestResults directories and packages.lock.json files
    /// from the current directory and all subdirectories.
    /// </remarks>
    /// <example>
    /// <code>
    /// dotnet easyaf cleanup
    /// dotnet easyaf cleanup --dry-run
    /// dotnet easyaf cleanup --path "C:\Projects\MyApp"
    /// </code>
    /// </example>
    [Command(Name = "cleanup", Description = "Clean up build artifacts (bin, obj, TestResults directories) and packages.lock.json files")]
    public class CleanupCommand
    {

        #region Properties

        /// <summary>
        /// Gets or sets the root directory to clean. Defaults to current directory.
        /// </summary>
        [Option("-p|--path", Description = "Root directory to clean (defaults to current directory)")]
        public string Path { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets or sets a value indicating whether to show what would be deleted without actually deleting.
        /// </summary>
        [Option("--dry-run", Description = "Show what would be deleted without actually deleting anything")]
        public bool DryRun { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to run in quiet mode with minimal output.
        /// </summary>
        [Option("-q|--quiet", Description = "Quiet mode - only show summary")]
        public bool Quiet { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the cleanup command.
        /// </summary>
        /// <returns>Exit code (0 for success, 1 for error).</returns>
        public async Task<int> OnExecuteAsync()
        {
            try
            {
                if (!Quiet)
                {
                    Console.WriteLine($"EasyAF Cleanup Tool");
                    Console.WriteLine($"Cleaning directory: {Path}");
                    if (DryRun)
                    {
                        Console.WriteLine("DRY RUN - No files will be deleted");
                    }
                    Console.WriteLine();
                }

                if (!Directory.Exists(Path))
                {
                    Console.Error.WriteLine($"Error: Directory '{Path}' does not exist.");
                    return 1;
                }

                var stats = new CleanupStats();
                CleanupDirectory(Path, stats);

                if (!Quiet || DryRun)
                {
                    Console.WriteLine();
                    Console.WriteLine("Cleanup Summary:");
                    Console.WriteLine($"  Directories processed: {stats.DirectoriesProcessed}");
                    Console.WriteLine($"  Bin directories {(DryRun ? "found" : "deleted")}: {stats.BinDirectoriesDeleted}");
                    Console.WriteLine($"  Obj directories {(DryRun ? "found" : "deleted")}: {stats.ObjDirectoriesDeleted}");
                    Console.WriteLine($"  TestResults directories {(DryRun ? "found" : "deleted")}: {stats.TestResultsDirectoriesDeleted}");
                    Console.WriteLine($"  packages.lock.json files {(DryRun ? "found" : "deleted")}: {stats.LockFilesDeleted}");
                    Console.WriteLine($"  Total space {(DryRun ? "that would be" : "")} freed: {FormatBytes(stats.BytesFreed)}");
                }

                await Task.CompletedTask.ConfigureAwait(false);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during cleanup: {ex.Message}");
                return 1;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Recursively cleans up the specified directory.
        /// </summary>
        /// <param name="directoryPath">The directory to clean.</param>
        /// <param name="stats">Statistics tracking object.</param>
        private void CleanupDirectory(string directoryPath, CleanupStats stats)
        {
            try
            {
                stats.DirectoriesProcessed++;

                var directoryInfo = new DirectoryInfo(directoryPath);
                var directoryName = directoryInfo.Name.ToLowerInvariant();

                // Check if this is a bin, obj, or TestResults directory
                if (directoryName == "bin" || directoryName == "obj" || directoryName == "testresults")
                {
                    var sizeBeforeDelete = GetDirectorySize(directoryPath);
                    
                    if (!Quiet)
                    {
                        if (DryRun)
                        {
                            Console.WriteLine($"Would delete {directoryName} directory: {directoryPath}");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Deleting {directoryName} directory: {directoryPath}");
                            Console.ResetColor();
                        }
                    }

                    if (!DryRun)
                    {
                        Directory.Delete(directoryPath, recursive: true);
                    }

                    stats.BytesFreed += sizeBeforeDelete;
                    
                    if (directoryName == "bin")
                    {
                        stats.BinDirectoriesDeleted++;
                    }
                    else if (directoryName == "obj")
                    {
                        stats.ObjDirectoriesDeleted++;
                    }
                    else if (directoryName == "testresults")
                    {
                        stats.TestResultsDirectoriesDeleted++;
                    }

                    // Don't recurse into bin/obj directories since we're deleting them
                    return;
                }

                // Look for packages.lock.json files in this directory
                var lockFilePath = System.IO.Path.Combine(directoryPath, "packages.lock.json");
                if (File.Exists(lockFilePath))
                {
                    var fileInfo = new FileInfo(lockFilePath);
                    var fileSize = fileInfo.Length;

                    if (!Quiet)
                    {
                        if (DryRun)
                        {
                            Console.WriteLine($"Would delete packages.lock.json: {lockFilePath}");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Deleting packages.lock.json: {lockFilePath}");
                            Console.ResetColor();
                        }
                    }

                    if (!DryRun)
                    {
                        File.Delete(lockFilePath);
                    }

                    stats.LockFilesDeleted++;
                    stats.BytesFreed += fileSize;
                }

                // Recursively clean subdirectories
                try
                {
                    foreach (var subdirectory in Directory.GetDirectories(directoryPath))
                    {
                        CleanupDirectory(subdirectory, stats);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    if (!Quiet)
                    {
                        Console.WriteLine($"Warning: Access denied to directory: {directoryPath}");
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    // Directory might have been deleted by a parent cleanup operation
                }
            }
            catch (UnauthorizedAccessException)
            {
                if (!Quiet)
                {
                    Console.WriteLine($"Warning: Access denied to directory: {directoryPath}");
                }
            }
            catch (Exception ex)
            {
                if (!Quiet)
                {
                    Console.WriteLine($"Warning: Error processing directory {directoryPath}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Calculates the total size of a directory and all its contents.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns>The total size in bytes.</returns>
        private static long GetDirectorySize(string directoryPath)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                long size = 0;

                // Calculate size of all files in this directory
                foreach (var file in directoryInfo.GetFiles())
                {
                    size += file.Length;
                }

                // Recursively calculate size of subdirectories
                foreach (var subdirectory in directoryInfo.GetDirectories())
                {
                    size += GetDirectorySize(subdirectory.FullName);
                }

                return size;
            }
            catch
            {
                // If we can't access the directory, return 0
                return 0;
            }
        }

        /// <summary>
        /// Formats bytes into a human-readable string.
        /// </summary>
        /// <param name="bytes">The number of bytes.</param>
        /// <returns>A formatted string (e.g., "1.5 MB").</returns>
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            
            if (bytes == 0)
            {
                return "0 B";
            }

            int suffixIndex = 0;
            double value = bytes;

            while (value >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                value /= 1024;
                suffixIndex++;
            }

            return $"{value:N1} {suffixes[suffixIndex]}";
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Tracks cleanup statistics.
        /// </summary>
        private class CleanupStats
        {
            public int DirectoriesProcessed { get; set; }
            public int BinDirectoriesDeleted { get; set; }
            public int ObjDirectoriesDeleted { get; set; }
            public int TestResultsDirectoriesDeleted { get; set; }
            public int LockFilesDeleted { get; set; }
            public long BytesFreed { get; set; }
        }

        #endregion

    }

}
