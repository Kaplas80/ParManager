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
    [Verb("add", HelpText = "Add (or replace) files in Yakuza PAR archive.")]
    internal class Add
    {
        /// <summary>
        /// Gets or sets the input directory.
        /// </summary>
        [Value(0, MetaName = "input", Required = true, HelpText = "Input PAR archive path.")]
        public string InputParArchivePath { get; set; }

        /// <summary>
        /// Gets or sets the path where the new files are located.
        /// </summary>
        [Value(1, MetaName = "path", Required = true, HelpText = "Folder to add.")]
        public string AddDirectory { get; set; }

        /// <summary>
        /// Gets or sets the PAR archive path.
        /// </summary>
        [Value(2, MetaName = "output", Required = true, HelpText = "New Yakuza PAR archive path.")]
        public string OutputParArchivePath { get; set; }

        /// <summary>
        /// Gets or sets the compression algorithm to use.
        /// </summary>
        [Option('c', "compression", Default = 0x01, HelpText = "SLLZ algorithm.")]
        public int Compression { get; set; }
    }
}