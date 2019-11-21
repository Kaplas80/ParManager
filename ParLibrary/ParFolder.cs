// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParLibrary
{
    using Yarhl.FileSystem;

    /// <summary>
    /// Represents a folder stored in a PAR archive.
    /// </summary>
    public class ParFolder : NodeContainerFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParFolder"/> class.
        /// </summary>
        public ParFolder()
        {
            this.FolderCount = 0;
            this.FirstFolderIndex = 0;
            this.FileCount = 0;
            this.FirstFileIndex = 0;
            this.Unknown1 = 0x00000010;
            this.Unknown2 = 0x00000000;
            this.Unknown3 = 0x00000000;
            this.Unknown4 = 0x00000000;
        }

        /// <summary>
        ///     Gets or sets the number of folders inside this folder.
        /// </summary>
        public int FolderCount { get; set; }

        /// <summary>
        ///     Gets or sets the first folder index inside this folder.
        /// </summary>
        public int FirstFolderIndex { get; set; }

        /// <summary>
        ///     Gets or sets the number of files inside this folder.
        /// </summary>
        public int FileCount { get; set; }

        /// <summary>
        ///     Gets or sets the first file index inside this folder.
        /// </summary>
        public int FirstFileIndex { get; set; }

        /// <summary>
        ///     Gets or sets the file "unknown" value #1.
        /// </summary>
        public int Unknown1 { get; set; }

        /// <summary>
        ///     Gets or sets the file "unknown" value #2.
        /// </summary>
        public int Unknown2 { get; set; }

        /// <summary>
        ///     Gets or sets the file "unknown" value #3.
        /// </summary>
        public int Unknown3 { get; set; }

        /// <summary>
        ///     Gets or sets the file "unknown" value #4.
        /// </summary>
        public int Unknown4 { get; set; }
    }
}
