// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParTool
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using ParLibrary;
    using ParLibrary.Converter;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Extract contents functionality.
    /// </summary>
    internal static partial class Program
    {
        private static void Extract(Options.Extract opts)
        {
            WriteHeader();

            if (!File.Exists(opts.ParArchivePath))
            {
                Console.WriteLine($"ERROR: \"{opts.ParArchivePath}\" not found!!!!");
                return;
            }

            if (Directory.Exists(opts.OutputDirectory))
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

            Directory.CreateDirectory(opts.OutputDirectory);

            // If a FilterRegex was specified (i.e. is not null) then make a new Regex using it. Otherwise, set filterRegex to null.
            var filterRegex = (opts.FilterRegex == null) ? null : new Regex(opts.FilterRegex);

            var parameters = new ParArchiveReaderParameters
            {
                Recursive = opts.Recursive,
            };

            using Node par = NodeFactory.FromFile(opts.ParArchivePath, Yarhl.IO.FileOpenMode.Read);
            par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(parameters);

            Extract(par, filterRegex, opts.OutputDirectory);
        }

        private static void Extract(Node parNode, Regex filterRegex, string outputFolder)
        {
            foreach (Node node in Navigator.IterateNodes(parNode))
            {
                string fileInfoPath = node.Path.Substring(parNode.Path.Length).Replace('/', Path.DirectorySeparatorChar);
                string outputPath = Path.Join(outputFolder, fileInfoPath);

                var file = node.GetFormatAs<ParFile>();

                if (file == null)
                {
                    Directory.CreateDirectory(outputPath);
                    continue;
                }

                // If the filterRegex exists, skip files that don't match it
                if (filterRegex != null && !filterRegex.IsMatch(node.Path))
                {
                    Console.WriteLine($"Skipping {node.Path} because it doesn't match the filter");
                    continue;
                }

                Console.Write($"Extracting {node.Path}... ");

                if (file.IsCompressed)
                {
                    node.TransformWith<ParLibrary.Sllz.Decompressor>();
                }

                if (node.Stream.Length > 0)
                {
                    node.Stream.WriteTo(outputPath);
                }
                else
                {
                    // Create empty file
                    File.Create(outputPath).Dispose();
                }

                File.SetCreationTime(outputPath, file.FileDate);
                File.SetLastWriteTime(outputPath, file.FileDate);
                File.SetAttributes(outputPath, (FileAttributes)file.Attributes);

                Console.WriteLine("DONE!");
            }
        }
    }
}
