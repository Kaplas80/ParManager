// -------------------------------------------------------
// © Kaplas, Samuel W. Stark (TheTurboTurnip). Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParTool.Options
{
    using CommandLine;

    /// <summary>
    /// PAR archive list options.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class is passed as type parameter.")]
    [Verb("list", HelpText = "Show contents from a Yakuza PAR archive.")]
    internal class List
    {
        /// <summary>
        /// Gets or sets the PAR archive path.
        /// </summary>
        [Value(0, MetaName = "archive", Required = true, HelpText = "Yakuza PAR archive path.")]
        public string ParArchivePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the list is recursive.
        /// </summary>
        [Option('r', "recursive", Default = false, HelpText = "List nested PAR archives.")]
        public bool Recursive { get; set; }

        /// <summary>
        /// Gets or sets a value used as a Regex filter for which files to list.
        /// </summary>
        [Option("filter", Default = null, HelpText = "Only list files that match this RegEx")]
        public string FilterRegex { get; set; }
    }
}
