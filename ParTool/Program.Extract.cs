// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParTool
{
    using System;
    using System.IO;
    using ParLib;
    using ParLib.Par.Converters;
    using Yarhl.FileSystem;

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

            using ParArchive parArchive = ParArchive.FromFile(opts.ParArchivePath);

            ExtractAll(parArchive, opts.OutputDirectory, string.Empty, opts.Recursive);
        }

        private static void ExtractAll(NodeContainerFormat parArchive, string outputFolder, string basePath = "", bool recursive = false)
        {
            foreach (Node node in Navigator.IterateNodes(parArchive.Root))
            {
                if (!(node.Format is ParLib.Par.FileInfo fileInfo))
                {
                    continue;
                }

                Console.Write($"Extracting {basePath}{fileInfo.Path}... ");

                string fileInfoPath = fileInfo.Path.Replace('/', Path.DirectorySeparatorChar);
                string outputPath = string.Concat(outputFolder, fileInfoPath);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                if (fileInfo.IsCompressed)
                {
                    node.TransformWith<ParLib.Sllz.Uncompressor>();
                }

                if (recursive && fileInfo.Name.EndsWith(".PAR", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine();

                    using var childContainer = node.TransformWith<ParArchiveReader>().GetFormatAs<ParArchive>();
                    string childOutputFolder = string.Concat(outputPath, ".unpack");
                    ExtractAll(childContainer, childOutputFolder, $"{basePath}{fileInfo.Path}", true);
                }
                else
                {
                    node.Stream.WriteTo(outputPath);
                    File.SetCreationTime(outputPath, fileInfo.FileDate);
                    File.SetLastWriteTime(outputPath, fileInfo.FileDate);
                }

                Console.WriteLine("DONE!");
            }
        }
    }
}
