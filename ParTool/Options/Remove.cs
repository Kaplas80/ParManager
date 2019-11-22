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
    [Verb("remove", HelpText = "Removes files or folders from Yakuza PAR archive.")]
    internal class Remove
    {
        /// <summary>
        /// Gets or sets the input directory.
        /// </summary>
        [Value(0, MetaName = "input", Required = true, HelpText = "Input PAR archive path.")]
        public string InputParArchivePath { get; set; }

        /// <summary>
        /// Gets or sets the PAR archive path.
        /// </summary>
        [Value(1, MetaName = "path", Required = true, HelpText = "File or folder to remove.")]
        public string RemovePath { get; set; }

        /// <summary>
        /// Gets or sets the PAR archive path.
        /// </summary>
        [Value(2, MetaName = "output", Required = true, HelpText = "New Yakuza PAR archive path.")]
        public string OutputParArchivePath { get; set; }
    }
}