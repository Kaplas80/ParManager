// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Kaplas80">
// © Kaplas80. Licensed under MIT. See LICENSE for details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace ParTool
{
    using System;
    using CommandLine;

    /// <summary>
    /// Main program.
    /// </summary>
    internal static partial class Program
    {
        private static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<ListOptions, ExtractOptions>(args)
                .WithParsed<ListOptions>(List)
                .WithParsed<ExtractOptions>(Extract);
        }

        private static void WriteHeader()
        {
            Console.WriteLine(CommandLine.Text.HeadingInfo.Default);
            Console.WriteLine(CommandLine.Text.CopyrightInfo.Default);
            Console.WriteLine();
        }
    }
}
