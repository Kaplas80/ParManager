// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParTool
{
    using System;
    using System.IO;
    using ParLib;
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

            using ParArchive parArchive = ParArchive.FromFile(opts.ParArchivePath);
            foreach (Node node in Navigator.IterateNodes(parArchive.Root))
            {
                if (node.Format is ParLib.Par.FileInfo fileInfo)
                {
                    Console.WriteLine($"{fileInfo.Path}\t{fileInfo.Size} bytes\t{fileInfo.FileDate:G}");
                }
            }
        }
    }
}
