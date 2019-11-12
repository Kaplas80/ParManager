// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="FileInfo.cs" company="Kaplas">
// © Kaplas. Licensed under MIT. See LICENSE for details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace ParLib.Par
{
    using System;
    using Yarhl.IO;

    /// <summary>
    /// File information stored in a .par archive.
    /// </summary>
    public class FileInfo : BinaryFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfo"/> class.
        /// </summary>
        public FileInfo()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfo"/> class.
        /// </summary>
        /// <param name="stream">Binary stream.</param>
        public FileInfo(DataStream stream)
            : base(stream)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfo"/> class.
        /// </summary>
        /// <param name="stream">Binary stream.</param>
        /// <param name="offset">Offset from the DataStream start.</param>
        /// <param name="length">Length of the substream.</param>
        public FileInfo(DataStream stream, long offset, long length)
            : base(stream, offset, length)
        {
        }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the file full path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file is compressed.
        /// </summary>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// Gets or sets the file size (uncompressed).
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the file size (compressed).
        /// </summary>
        public int CompressedSize { get; set; }

        /// <summary>
        /// Gets or sets the file offset inside .par archive.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the file "unknown" value #1.
        /// </summary>
        public int Unknown1 { get; set; }

        /// <summary>
        /// Gets or sets the file "unknown" value #2.
        /// </summary>
        public int Unknown2 { get; set; }

        /// <summary>
        /// Gets or sets the file "unknown" value #3.
        /// </summary>
        public int Unknown3 { get; set; }

        /// <summary>
        /// Gets or sets the file date (as integer).
        /// </summary>
        public int Date { get; set; }

        /// <summary>
        /// Gets the file date (as DateTime).
        /// </summary>
        public DateTime FileDate
        {
            get
            {
                var baseDate = new DateTime(1970, 1, 1);
                return baseDate.AddSeconds(this.Date);
            }
        }
    }
}
