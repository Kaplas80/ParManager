// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.List.cs" company="Kaplas80">
// © Kaplas80. Licensed under MIT. See LICENSE for details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace ParTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using CommandLine;

    /// <summary>
    /// List contents functionality.
    /// </summary>
    internal static partial class Program
    {
        private static void List(ListOptions opts)
        {
            WriteHeader();

            if (!File.Exists(opts.ParFile))
            {
                Console.WriteLine($"ERROR: \"{opts.ParFile}\" not found!!!!");
                return;
            }

            IList<ParLib.Par.FileInfo> parContents = ParLib.Api.GetParContents(opts.ParFile);

            foreach (ParLib.Par.FileInfo info in parContents)
            {
                Console.WriteLine($"{info.Path}\t{info.Size} bytes\t{info.FileDate:G}");
            }
        }

        [Verb("list", HelpText = "Show contents from a Yakuza PAR file.")]
        private class ListOptions
        {
            [Option('i', "input", Required = true, HelpText = "Yakuza PAR file")]
            public string ParFile { get; set; }
        }
    }
}
