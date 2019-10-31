// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.Extract.cs" company="Kaplas80">
// © Kaplas80. Licensed under MIT. See LICENSE for details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace ParTool
{
    using System;
    using System.IO;
    using CommandLine;

    /// <summary>
    /// Extract contents functionality.
    /// </summary>
    internal static partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:No pasar cadenas literal como parámetros localizados", Justification = "<pendiente>")]
        private static void Extract(ExtractOptions opts)
        {
            WriteHeader();

            if (!File.Exists(opts.ParFile))
            {
                Console.WriteLine($"ERROR: \"{opts.ParFile}\" not found!!!!");
                return;
            }

            if (Directory.Exists(opts.OutputFolder))
            {
                Console.WriteLine("WARNING: Output directory already exists. Its contents may be overwritten.");
                Console.Write("Continue? (y/N) ");
                string answer = Console.ReadLine();
                if (!string.IsNullOrEmpty(answer) && answer.ToUpperInvariant() != "Y")
                {
                    Console.WriteLine("CANCELLED BY USER.");
                    return;
                }
            }

            Directory.CreateDirectory(opts.OutputFolder);

            ParLib.Api.OnFileExtracting += (sender, info) =>
            {
                Console.WriteLine($"Extracting {info.Path}...");
            };

            ParLib.Api.OnFileExtracted += (sender, info) =>
            {
                Console.WriteLine($"Extracted {info.Path}...");
            };

            ParLib.Api.Extract(opts.ParFile, opts.OutputFolder, opts.Recursive);
        }

        [Verb("extract", HelpText = "Extract contents from a Yakuza PAR file.")]
        private class ExtractOptions
        {
            [Option('i', "input", Required = true, HelpText = "Yakuza PAR file")]
            public string ParFile { get; set; }

            [Option('o', "output", Required = true, HelpText = "Output directory")]
            public string OutputFolder { get; set; }

            [Option("recursive", Required = false, Default = false, HelpText = "Extract recursively")]
            public bool Recursive { get; set; }
        }
    }
}
