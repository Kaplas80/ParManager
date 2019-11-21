// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParTool
{
    using System;
    using System.IO;
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

            var parameters = new ParArchiveReaderParameters
            {
                Recursive = opts.Recursive,
            };

            using Node par = NodeFactory.FromFile(opts.ParArchivePath);
            par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(parameters);

            var extractionNode = new Node(".", par.Format);
            Extract(extractionNode, opts.OutputDirectory);
        }

        private static void Extract(Node parNode, string outputFolder)
        {
            foreach (Node node in Navigator.IterateNodes(parNode))
            {
                var file = node.GetFormatAs<BinaryFormat>();
                if (file == null)
                {
                    continue;
                }

                Console.Write($"Extracting {node.Path}... ");

                string fileInfoPath = node.Path.Replace('/', Path.DirectorySeparatorChar);
                string outputPath = Path.Join(outputFolder, fileInfoPath);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                if (node.Tags["IsCompressed"])
                {
                    node.TransformWith<ParLibrary.Sllz.Decompressor>();
                }

                node.Stream.WriteTo(outputPath);
                File.SetAttributes(outputPath, node.Tags["Attributes"]);
                File.SetCreationTime(outputPath, node.Tags["FileDate"]);
                File.SetLastWriteTime(outputPath, node.Tags["FileDate"]);

                Console.WriteLine("DONE!");
            }
        }
    }
}
