// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="FolderInfo.cs" company="Kaplas">
// © Kaplas. Licensed under MIT. See LICENSE for details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace ParLib.Par
{
    /// <summary>
    ///     Folder information stored in a .par archive.
    /// </summary>
    public class FolderInfo
    {
        /// <summary>
        /// Gets or sets the folder name.
        /// </summary>
        public string Name { get; set; }

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