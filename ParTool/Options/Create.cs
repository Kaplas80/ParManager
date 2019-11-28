// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParTool.Options
{
    using CommandLine;

    /// <summary>
    /// PAR archive extract options.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is passed as type parameter.")]
    [Verb("create", HelpText = "Create a Yakuza PAR archive.")]
    internal class Create
    {
        /// <summary>
        /// Gets or sets the input directory.
        /// </summary>
        [Value(0, MetaName = "input", Required = true, HelpText = "Input directory.")]
        public string InputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the PAR archive path.
        /// </summary>
        [Value(1, MetaName = "archive", Required = true, HelpText = "New Yakuza PAR archive path.")]
        public string ParArchivePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output file will be used in a PS3 game.
        /// </summary>
        [Option("ps3", Default = false, HelpText = "Use it if the PAR is for a PS3 game.")]
        public bool Ps3Mode { get; set; }

        /// <summary>
        /// Gets or sets the compression algorithm to use.
        /// </summary>
        [Option('c', "compression", Default = 0x01, HelpText = "SLLZ algorithm.")]
        public int Compression { get; set; }
    }
}