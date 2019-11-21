// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParTool
{
    using System;
    using System.IO;
    using ParLibrary.Converter;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// List contents functionality.
    /// </summary>
    internal static partial class Program
    {
        private static void List(Options.List opts)
        {
            WriteHeader();

            if (!File.Exists(opts.ParArchivePath))
            {
                Console.WriteLine($"ERROR: \"{opts.ParArchivePath}\" not found!!!!");
                return;
            }

            var parameters = new ParArchiveReaderParameters
            {
                Recursive = opts.Recursive,
            };

            using Node par = NodeFactory.FromFile(opts.ParArchivePath);
            par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(parameters);

            foreach (Node node in Navigator.IterateNodes(par))
            {
                var file = node.GetFormatAs<BinaryFormat>();
                if (file != null)
                {
                    Console.WriteLine($"{node.Path}\t{node.Tags["DecompressedSize"]} bytes\t{node.Tags["FileDate"]:G}");
                }
            }
        }
    }
}
