// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Api.List.cs" company="Kaplas80">
// © Kaplas80. Licensed under MIT. See LICENSE for details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace ParLib
{
    using System.Collections.Generic;
    using ParLib.Par.Converters;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// PAR list methods.
    /// </summary>
    public static partial class Api
    {
        /// <summary>
        /// Gets a list of Yakuza PAR archive contents.
        /// </summary>
        /// <param name="parArchive">Full path to the PAR archive.</param>
        /// <returns>A list with all the files details.</returns>
        public static IList<ParLib.Par.FileInfo> GetParContents(string parArchive)
        {
            DataStream parDataStream = DataStreamFactory.FromFile(parArchive, FileOpenMode.Read);
            using var parBinaryFormat = new BinaryFormat(parDataStream);

            var nodeContainer = (NodeContainerFormat)ConvertFormat.With<ParBinaryToNodeContainer>(parBinaryFormat);

            var result = new List<ParLib.Par.FileInfo>();
            foreach (Node node in Navigator.IterateNodes(nodeContainer.Root))
            {
                if (node.Format is ParLib.Par.FileInfo fileInfo)
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
