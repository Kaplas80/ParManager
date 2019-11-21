// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParTool
{
    using System;
    using System.IO;
    using ParLibrary.Converter;
    using Yarhl.FileSystem;

    /// <summary>
    /// Create contents functionality.
    /// </summary>
    internal static partial class Program
    {
        private static void Create(Options.Create opts)
        {
            WriteHeader();

            if (!Directory.Exists(opts.InputDirectory))
            {
                Console.WriteLine($"ERROR: \"{opts.InputDirectory}\" not found!!!!");
                return;
            }

            if (File.Exists(opts.ParArchivePath))
            {
                Console.WriteLine("WARNING: Output file already exists. It will be overwritten.");
                Console.Write("Continue? (y/N) ");
                string answer = Console.ReadLine();
                if (!string.IsNullOrEmpty(answer) && answer.ToUpperInvariant() != "Y")
                {
                    Console.WriteLine("CANCELLED BY USER.");
                    return;
                }

                File.Delete(opts.ParArchivePath);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(opts.ParArchivePath));

            //string name = new DirectoryInfo(opts.ParArchivePath).Name;

            Node node = ParLibrary.NodeFactory.FromDirectory(opts.InputDirectory, "*", ".", true);
            node.TransformWith<ParArchiveWriter>();
            node.Stream.WriteTo(opts.ParArchivePath);
        }
    }
}
