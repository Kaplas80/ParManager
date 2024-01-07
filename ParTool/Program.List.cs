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

                // If we encounter a zero-length PAR at any point, we treat it as an empty directory.
                AllowZeroLengthPars = true,
            };

            using Node par = NodeFactory.FromFile(opts.ParArchivePath, Yarhl.IO.FileOpenMode.Read);

            // For convenience, warn the user if the top-level PAR they're using is a zero-length file.
            // We still use the AllowZeroLengthPARs parameter, in case a non-zero-length PAR contains a zero-length PAR and we're reading in recursive mode.
            if (par.Stream.Length == 0)
            {
                Console.WriteLine($"WARNING: \"{opts.ParArchivePath}\" is an empty file, and contains no data.");
            }

            par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(parameters);

            foreach (Node node in Navigator.IterateNodes(par))
            {
                var file = node.GetFormatAs<ParFile>();
                if (file != null)
                {
                    var compression = file.IsCompressed ? "*" : string.Empty;
                    Console.WriteLine($"{node.Path}{compression}\t{file.DecompressedSize} bytes\t{file.FileDate:G}");
                }
            }
        }
    }
}
