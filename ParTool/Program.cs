// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParTool
{
    using System;
    using System.IO;
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// Main program.
    /// </summary>
    internal static partial class Program
    {
        private static void Main(string[] args)
        {
            using var parser = new Parser(with => with.HelpWriter = null);
            ParserResult<object> parserResult = parser.ParseArguments<Options.List, Options.Extract, Options.Create, Options.Remove, Options.Add>(args);
            parserResult
                .WithParsed<Options.List>(List)
                .WithParsed<Options.Extract>(Extract)
                .WithParsed<Options.Create>(Create)
                .WithParsed<Options.Remove>(Remove)
                .WithParsed<Options.Add>(Add)
                .WithNotParsed(x =>
                {
                    if (args.Length == 1)
                    {
                        if (File.Exists(args[0]))
                        {
                            var opts = new Options.Extract
                            {
                                ParArchivePath = args[0],
                                OutputDirectory = string.Concat(args[0], ".unpack"),
                                Recursive = true,
                            };

                            Extract(opts);
                            return;
                        }

                        if (Directory.Exists(args[0]))
                        {
                            var opts = new Options.Create
                            {
                                InputDirectory = args[0],
                                ParArchivePath =
                                    args[0].EndsWith(".unpack", StringComparison.InvariantCultureIgnoreCase)
                                        ? args[0].Substring(0, args[0].Length - 7)
                                        : string.Concat(args[0], ".par"),
                                AlternativeMode = false,
                                Compression = 1,
                            };

                            Create(opts);
                            return;
                        }
                    }

                    HelpText helpText = HelpText.AutoBuild(
                        parserResult,
                        h =>
                        {
                            h.AutoHelp = false; // hide --help
                            h.AutoVersion = false; // hide --version
                            return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                        },
                        e => e);

                    Console.WriteLine(helpText);
                });
        }

        private static void WriteHeader()
        {
            Console.WriteLine(CommandLine.Text.HeadingInfo.Default);
            Console.WriteLine(CommandLine.Text.CopyrightInfo.Default);
            Console.WriteLine();
        }
    }
}
