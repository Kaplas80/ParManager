// -------------------------------------------------------
// © Kaplas, Samuel W. Stark (TheTurboTurnip). Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParTool
{
    using System;
    using System.IO;
    using ParLibrary.Converter;
    using Yarhl.FileSystem;

    /// <summary>
    /// Node removal functionality.
    /// </summary>
    internal static partial class Program
    {
        private static void Remove(Options.Remove opts)
        {
            WriteHeader();

            if (!File.Exists(opts.InputParArchivePath))
            {
                Console.WriteLine($"ERROR: \"{opts.InputParArchivePath}\" not found!!!!");
                return;
            }

            if (File.Exists(opts.OutputParArchivePath))
            {
                Console.WriteLine("WARNING: Output file already exists. It will be overwritten.");
                Console.Write("Continue? (y/N) ");
                string answer = Console.ReadLine();
                if (!string.IsNullOrEmpty(answer) && answer.ToUpperInvariant() != "Y")
                {
                    Console.WriteLine("CANCELLED BY USER.");
                    return;
                }

                File.Delete(opts.OutputParArchivePath);
            }

            var readerParameters = new ParArchiveReaderParameters
            {
                Recursive = true,

                // If we encounter a zero-length PAR at any point, we treat it as an empty directory.
                AllowZeroLengthPars = true,
            };

            using Node par = NodeFactory.FromFile(opts.InputParArchivePath, Yarhl.IO.FileOpenMode.Read);
            par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(readerParameters);

            Node foundNode = Navigator.SearchNode(par, opts.RemovePath);

            if (foundNode == null)
            {
                Console.WriteLine($"ERROR: \"{opts.RemovePath}\" not found in PAR.");
                return;
            }

            if (foundNode.Parent == null)
            {
                Console.WriteLine("ERROR: Cannot remove PAR root node.");
                return;
            }

            Console.WriteLine("Removing...");
            Node parent = foundNode.Parent;
            parent.Remove(foundNode.Name);

            while (parent.Parent != null && parent.Children.Count == 0)
            {
                foundNode = parent;
                parent = parent.Parent;
                parent.Remove(foundNode);
                foundNode.Dispose();
            }

            par.TransformWith<ParArchiveWriter>();

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(opts.OutputParArchivePath)));
            par.Stream.WriteTo(opts.OutputParArchivePath);
            Console.WriteLine("DONE!");
        }
    }
}
