// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Api.cs" company="Kaplas80">
// © Kaplas80. Licensed under MIT. See LICENSE for details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace ParLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
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
        /// Executed when a file has been processed.
        /// </summary>
        public static event EventHandler<ParLib.Par.FileInfo> OnFileProcessed = (sender, info) => { };

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

        /// <summary>
        /// Extracts the contents of a Yakuza PAR archive.
        /// </summary>
        /// <param name="parArchive">Full path to the PAR archive.</param>
        /// <param name="outputFolder">Directory to write the contents.</param>
        /// <param name="recursive">If true, it will extract contained PAR files.</param>
        public static void Extract(string parArchive, string outputFolder, in bool recursive)
        {
            DataStream parDataStream = DataStreamFactory.FromFile(parArchive, FileOpenMode.Read);
            using var parBinaryFormat = new BinaryFormat(parDataStream);

            var nodeContainer = (NodeContainerFormat)ConvertFormat.With<ParBinaryToNodeContainer>(parBinaryFormat);

            Extract(nodeContainer, outputFolder, recursive);

            nodeContainer.Root.Dispose();
            nodeContainer.Dispose();
        }

        private static void Extract(NodeContainerFormat nodeContainer, string outputFolder, in bool recursive)
        {
            foreach (Node node in Navigator.IterateNodes(nodeContainer.Root))
            {
                if (!(node.Format is ParLib.Par.FileInfo fileInfo))
                {
                    continue;
                }

                string fileInfoPath = fileInfo.Path.Replace('/', Path.DirectorySeparatorChar);
                string outputPath = string.Concat(outputFolder, fileInfoPath);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                if (fileInfo.IsCompressed)
                {
                    if (recursive && fileInfo.Name.EndsWith(".PAR", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var childContainer = node.TransformWith<Sllz.Uncompressor>().TransformWith<ParBinaryToNodeContainer>().GetFormatAs<NodeContainerFormat>();
                        string childOutputFolder = string.Concat(outputPath, ".unpack");
                        Extract(childContainer, childOutputFolder, true);
                        childContainer.Root.Dispose();
                        childContainer.Dispose();
                    }
                    else
                    {
                        node.TransformWith<Sllz.Uncompressor>().Stream.WriteTo(outputPath);
                    }
                }
                else
                {
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
                    }
                }

                if (File.Exists(outputPath))
                {
                    File.SetCreationTime(outputPath, fileInfo.FileDate);
                    File.SetLastWriteTime(outputPath, fileInfo.FileDate);
                }

                OnFileProcessed(null, fileInfo);
            }
        }
    }
}
