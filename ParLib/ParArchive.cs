// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParLib
{
    using System.Text;
    using ParLib.Par.Converters;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Represents a PAR archive.
    /// </summary>
    public class ParArchive : NodeContainerFormat
    {
        static ParArchive()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Reads a PAR archive from a file.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The ParArchive structure.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Dispose ownership transfer.")]
        public static ParArchive FromFile(string fileName)
        {
            DataStream parDataStream = DataStreamFactory.FromFile(fileName, FileOpenMode.Read);
            var parBinaryFormat = new BinaryFormat(parDataStream);
            return (ParArchive)ConvertFormat.With<ParArchiveReader>(parBinaryFormat);
        }
    }
}
