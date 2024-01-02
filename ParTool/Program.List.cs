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

            // If a FilterRegex was specified (i.e. is not null) then make a new Regex using it. Otherwise, set filterRegex to null.
            var filterRegex = (opts.FilterRegex == null) ? null : new Regex(opts.FilterRegex);

            var parameters = new ParArchiveReaderParameters
            {
                Recursive = opts.Recursive,
            };

            using Node par = NodeFactory.FromFile(opts.ParArchivePath, Yarhl.IO.FileOpenMode.Read);
            par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(parameters);

            foreach (Node node in Navigator.IterateNodes(par))
            {
                var file = node.GetFormatAs<ParFile>();
                if (file != null)
                {
                    // If the filterRegex exists, skip files that don't match it
                    if (filterRegex != null && !filterRegex.IsMatch(node.Path))
                    {
                        continue;
                    }

                    var compression = file.IsCompressed ? "*" : string.Empty;
                    Console.WriteLine($"{node.Path}{compression}\t{file.DecompressedSize} bytes\t{file.FileDate:G}");
                }
            }
        }
    }
}
