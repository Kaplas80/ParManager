// -------------------------------------------------------
// © Kaplas, Samuel W. Stark (TheTurboTurnip). Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParTool.Options
{
    using CommandLine;

    /// <summary>
    /// PAR archive extract options.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is passed as type parameter.")]
    [Verb("extract", HelpText = "Extract contents from a Yakuza PAR archive.")]
    internal class Extract
    {
        /// <summary>
        /// Gets or sets the PAR archive path.
        /// </summary>
        [Value(0, MetaName = "archive", Required = true, HelpText = "Yakuza PAR archive path.")]
        public string ParArchivePath { get; set; }

        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        [Value(1, MetaName = "path_to_extract\\", Required = true, HelpText = "Output directory.")]
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the extraction is recursive.
        /// </summary>
        [Option('r', "recursive", Default = false, HelpText = "Extract nested PAR archives.")]
        public bool Recursive { get; set; }

        /// <summary>
        /// Gets or sets a value used as a Regex filter for which real-files to extract.
        /// </summary>
        [Option("filter", Default = null, HelpText = "Only extract files that match this RegEx (directories will always be extracted)")]
        public string FilterRegex { get; set; }
    }
}
