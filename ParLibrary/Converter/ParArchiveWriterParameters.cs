﻿// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParLibrary.Converter
{
    /// <summary>
    /// Parameters for ParArchiveWriter.
    /// </summary>
    public class ParArchiveWriterParameters
    {
        /// <summary>
        /// Gets or sets the compressor version to use.
        /// </summary>
        public int CompressorVersion { get; set; }

        /// <summary>
        /// Gets or sets the path to write.
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "." nodes are needed.
        /// </summary>
        public bool IncludeDots { get; set; }
    }
}
