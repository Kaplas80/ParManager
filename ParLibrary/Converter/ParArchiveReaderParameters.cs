// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParLibrary.Converter
{
    /// <summary>
    /// Parameters for ParArchiveReader.
    /// </summary>
    public class ParArchiveReaderParameters
    {
        /// <summary>
        /// Gets or sets a value indicating whether the reading is recursive.
        /// </summary>
        public bool Recursive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether zero-length PARs cause an error or not.
        /// </summary>
        public bool AllowZeroLengthPars { get; set; }
    }
}
