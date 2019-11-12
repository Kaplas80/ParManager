// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ParBinaryToNodeContainer.cs" company="Kaplas">
// © Kaplas. Licensed under MIT. See LICENSE for details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace ParLib.Par.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Converter for Binary streams into a file system following the
    /// PARC tree format.
    /// </summary>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownership dispose transferred")]
    public class ParBinaryToNodeContainer : IConverter<BinaryFormat, NodeContainerFormat>
    {
        /// <summary>
        /// Converts a binary stream into a file system with the Par format.
        /// </summary>
        /// <param name="source">The binary stream to convert.</param>
        /// <returns>The file system from the PARC stream.</returns>
        public NodeContainerFormat Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var reader = new DataReader(source.Stream)
            {
                DefaultEncoding = Encoding.GetEncoding(1252),
                Endianness = EndiannessMode.BigEndian,
            };

            if (reader.ReadString(4) != "PARC")
            {
                throw new FormatException("PARC: Bad magic Id.");
            }

            if (reader.ReadInt32() != 0x02010000)
            {
                throw new FormatException("PARC: Bad unknown value #1.");
            }

            if (reader.ReadInt32() != 0x00020001)
            {
                throw new FormatException("PARC: Bad unknown value #2.");
            }

            if (reader.ReadInt32() != 0x00000000)
            {
                throw new FormatException("PARC: Bad unknown value #3.");
            }

            int totalFolderCount = reader.ReadInt32();
            int folderInfoOffset = reader.ReadInt32();
            int totalFileCount = reader.ReadInt32();
            int fileInfoOffset = reader.ReadInt32();

            var folderNames = new string[totalFolderCount];
            for (int i = 0; i < totalFolderCount; i++)
            {
                folderNames[i] = reader.ReadString(0x40).TrimEnd('\0');
            }

            var fileNames = new string[totalFileCount];
            for (int i = 0; i < totalFileCount; i++)
            {
                fileNames[i] = reader.ReadString(0x40).TrimEnd('\0');
            }

            reader.Stream.Seek(folderInfoOffset);
            var folderInfos = new FolderInfo[totalFolderCount];

            folderInfos[0] = new FolderInfo
            {
                Name = folderNames[0],
                FolderCount = reader.ReadInt32(),
                FirstFolderIndex = reader.ReadInt32(),
                FileCount = reader.ReadInt32(),
                FirstFileIndex = reader.ReadInt32(),
                Unknown1 = reader.ReadInt32(),
                Unknown2 = reader.ReadInt32(),
                Unknown3 = reader.ReadInt32(),
                Unknown4 = reader.ReadInt32(),
            };

            for (int i = 1; i < totalFolderCount; i++)
            {
                folderInfos[i] = new FolderInfo
                {
                    Name = folderNames[i],
                    FolderCount = reader.ReadInt32(),
                    FirstFolderIndex = reader.ReadInt32(),
                    FileCount = reader.ReadInt32(),
                    FirstFileIndex = reader.ReadInt32(),
                    Unknown1 = reader.ReadInt32(),
                    Unknown2 = reader.ReadInt32(),
                    Unknown3 = reader.ReadInt32(),
                    Unknown4 = reader.ReadInt32(),
                };
            }

            reader.Stream.Seek(fileInfoOffset);
            var fileInfos = new FileInfo[totalFileCount];
            for (int i = 0; i < totalFileCount; i++)
            {
                uint compressionFlag = reader.ReadUInt32();
                int size = reader.ReadInt32();
                int compressedSize = reader.ReadInt32();
                int offset = reader.ReadInt32();
                int unknown1 = reader.ReadInt32();
                int unknown2 = reader.ReadInt32();
                int unknown3 = reader.ReadInt32();
                int date = reader.ReadInt32();

                fileInfos[i] = new FileInfo(source.Stream, offset, compressedSize)
                {
                    Name = fileNames[i],
                    IsCompressed = compressionFlag == 0x80000000,
                    Size = size,
                    CompressedSize = compressedSize,
                    Offset = offset,
                    Unknown1 = unknown1,
                    Unknown2 = unknown2,
                    Unknown3 = unknown3,
                    Date = date,
                };
            }

            return BuildContainer(folderInfos, fileInfos);
        }

        private static NodeContainerFormat BuildContainer(IReadOnlyList<FolderInfo> folderInfos, IReadOnlyList<FileInfo> fileInfos)
        {
            Node root = NodeFactory.CreateContainer(folderInfos[0].Name);

            BuildNode(root, folderInfos[0], folderInfos, fileInfos);

            return root.Format as NodeContainerFormat;
        }

        private static void BuildNode(Node node, FolderInfo info, IReadOnlyList<FolderInfo> folderInfos, IReadOnlyList<FileInfo> fileInfos)
        {
            for (int i = info.FirstFolderIndex; i < info.FirstFolderIndex + info.FolderCount; i++)
            {
                var child = new Node(folderInfos[i].Name);
                node.Add(child);
                BuildNode(child, folderInfos[i], folderInfos, fileInfos);
            }

            for (int i = info.FirstFileIndex; i < info.FirstFileIndex + info.FileCount;
                i++)
            {
                var child = new Node(fileInfos[i].Name, fileInfos[i]);
                node.Add(child);
                fileInfos[i].Path = child.Path;
            }
        }
    }
}
