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
    /// Node adding functionality.
    /// </summary>
    internal static partial class Program
    {
        private static void Add(Options.Add opts)
        {
            WriteHeader();

            if (!File.Exists(opts.InputParArchivePath))
            {
                Console.WriteLine($"ERROR: \"{opts.InputParArchivePath}\" not found!!!!");
                return;
            }

            if (!Directory.Exists(opts.AddDirectory))
            {
                Console.WriteLine($"ERROR: \"{opts.AddDirectory}\" not found!!!!");
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

                // If we encounter a zero-length PAR at any point below the top level, we treat it as an empty directory.
                AllowZeroLengthPars = true,
            };

            var writerParameters = new ParArchiveWriterParameters
            {
                CompressorVersion = opts.Compression,
                OutputPath = string.Empty,
            };

            Console.Write("Reading PAR file... ");
            Node par = NodeFactory.FromFile(opts.InputParArchivePath, Yarhl.IO.FileOpenMode.Read);

            // Warn the user if the top-level PAR they're using is a zero-length file.
            // If it is, we can't infer the IncludeDots parameter.
            if (par.Stream.Length == 0)
            {
                Console.WriteLine($"ERROR: \"{opts.InputParArchivePath}\" is an empty file, and contains no data. Use `ParTool.exe create` instead.");
                return;
            }

            par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(readerParameters);
            writerParameters.IncludeDots = (par.Children.Count > 0) && par.Children[0].Name == ".";
            Console.WriteLine("DONE!");

            Console.Write("Reading input directory... ");
            string nodeName = new DirectoryInfo(opts.AddDirectory).Name;
            Node node = ReadDirectory(opts.AddDirectory, nodeName);
            node.TransformWith<ParArchiveWriter, ParArchiveWriterParameters>(writerParameters).TransformWith<ParArchiveReader, ParArchiveReaderParameters>(readerParameters);
            Console.WriteLine("DONE!");

            Console.Write("Adding files... ");
            node.GetFormatAs<NodeContainerFormat>().MoveChildrenTo(par, true);
#pragma warning disable CA1308 // Normalize strings to uppercase
            par.SortChildren((x, y) => string.CompareOrdinal(x.Name.ToLowerInvariant(), y.Name.ToLowerInvariant()));
#pragma warning restore CA1308 // Normalize strings to uppercase
            Console.WriteLine("DONE!");

            ParArchiveWriter.NestedParCreating += sender => Console.WriteLine($"Creating nested PAR {sender.Name}... ");
            ParArchiveWriter.NestedParCreated += sender => Console.WriteLine($"{sender.Name} created!");
            ParArchiveWriter.FileCompressing += sender => Console.WriteLine($"Compressing {sender.Name}... ");

            Console.WriteLine("Creating PAR (this may take a while)... ");
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(opts.OutputParArchivePath)));
            writerParameters.OutputPath = opts.OutputParArchivePath;
            writerParameters.IncludeDots = false;
            par.TransformWith<ParArchiveWriter, ParArchiveWriterParameters>(writerParameters);
            par.Dispose();
            node.Dispose();

            Console.WriteLine("DONE!");
        }
    }
}
