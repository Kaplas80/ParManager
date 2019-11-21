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

            Console.Write("Reading input directory... ");
            Node node = NodeFactory.FromDirectory(opts.InputDirectory, "*", ".", true);
            Console.WriteLine("DONE!");

            ParArchiveWriter.NestedParCreating += sender => Console.WriteLine($"Creating nested PAR {sender.Name}... ");
            ParArchiveWriter.NestedParCreated += sender => Console.WriteLine($"{sender.Name} created!");
            ParArchiveWriter.FileCompressing += sender => Console.WriteLine($"Compressing {sender.Name}... ");

            Console.WriteLine("Creating PAR (this may take a while)... ");
            node.TransformWith<ParArchiveWriter>();
            node.Stream.WriteTo(opts.ParArchivePath);
            Console.WriteLine("DONE!");
        }
    }
}
