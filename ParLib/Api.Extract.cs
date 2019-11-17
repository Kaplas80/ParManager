// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Api.Extract.cs" company="Kaplas">
// © Kaplas. Licensed under MIT. See LICENSE for details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace ParLib
{
    using System;
    using System.IO;
    using ParLib.Par.Converters;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// PAR extraction methods.
    /// </summary>
    public static partial class Api
    {
        /// <summary>
        /// Executed before a file has been extracted.
        /// </summary>
        public static event EventHandler<ParLib.Par.FileInfo> OnFileExtracting = (sender, info) => { };

        /// <summary>
        /// Executed after a file has been extracted.
        /// </summary>
        public static event EventHandler<ParLib.Par.FileInfo> OnFileExtracted = (sender, info) => { };

        /// <summary>
        /// Extracts the contents of a Yakuza PAR archive.
        /// </summary>
        /// <param name="parArchive">Full path to the PAR archive.</param>
        /// <param name="outputFolder">Directory to write the contents.</param>
        /// <param name="recursive">If true, it will extract contained PAR files.</param>
        public static void Extract(string parArchive, string outputFolder, in bool recursive)
        {
            DataStream parDataStream = DataStreamFactory.FromFile(parArchive, FileOpenMode.Read);
            using (var parBinaryFormat = new BinaryFormat(parDataStream))
            {
                var nodeContainer = (NodeContainerFormat)ConvertFormat.With<ParBinaryToNodeContainer>(parBinaryFormat);

                Extract(nodeContainer, outputFolder, recursive);

                nodeContainer.Root.Dispose();
                nodeContainer.Dispose();
            }
        }

        private static void Extract(NodeContainerFormat nodeContainer, string outputFolder, in bool recursive)
        {
            foreach (Node node in Navigator.IterateNodes(nodeContainer.Root))
            {
                if (!(node.Format is ParLib.Par.FileInfo fileInfo))
                {
                    continue;
                }

                OnFileExtracting(null, fileInfo);

                string fileInfoPath = fileInfo.Path.Replace('/', Path.DirectorySeparatorChar);
                string outputPath = string.Concat(outputFolder, fileInfoPath);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                if (fileInfo.IsCompressed)
                {
                    node.TransformWith<Sllz.Uncompressor>();
                }

                if (recursive && fileInfo.Name.EndsWith(".PAR", StringComparison.InvariantCultureIgnoreCase))
                {
                    var childContainer = node.TransformWith<ParBinaryToNodeContainer>().GetFormatAs<NodeContainerFormat>();
                    string childOutputFolder = string.Concat(outputPath, ".unpack");
                    Extract(childContainer, childOutputFolder, true);
                    childContainer.Root.Dispose();
                    childContainer.Dispose();
                }
                else
                {
                    node.Stream.WriteTo(outputPath);
                    File.SetCreationTime(outputPath, fileInfo.FileDate);
                    File.SetLastWriteTime(outputPath, fileInfo.FileDate);
                }

                OnFileExtracted(null, fileInfo);
            }
        }
    }
}
