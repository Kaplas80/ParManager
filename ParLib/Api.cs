// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Api.cs" company="Kaplas80">
// © Kaplas80. Licensed under MIT. See LICENSE for details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace ParLib
{
    using System.Collections.Generic;
    using System.Text;
    using ParLib.Par;
    using ParLib.Par.Converters;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Exposes the public functionality of the library.
    /// </summary>
    public static class Api
    {
        static Api()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Gets a list of .par archive contents.
        /// </summary>
        /// <param name="parArchive">Full path to the .par archive.</param>
        /// <returns>A list with all the files details.</returns>
        public static IList<FileInfo> GetParContents(string parArchive)
        {
            DataStream parDataStream = DataStreamFactory.FromFile(parArchive, FileOpenMode.Read);
            using var parBinaryFormat = new BinaryFormat(parDataStream);

            var nodeContainer = (NodeContainerFormat)ConvertFormat.With<ParBinaryToNodeContainer>(parBinaryFormat);

            var result = new List<FileInfo>();
            foreach (Node node in Navigator.IterateNodes(nodeContainer.Root))
            {
                if (node.Format is FileInfo fileInfo)
                {
                    result.Add(fileInfo);
                }
            }

            nodeContainer.Root.Dispose();
            nodeContainer.Dispose();

            return result;
        }
    }
}
