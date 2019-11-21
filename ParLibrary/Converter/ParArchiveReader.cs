// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParLibrary.Converter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Converter from BinaryFormat to ParArchive.
    /// </summary>
    public class ParArchiveReader : IInitializer<ParArchiveReaderParameters>, IConverter<BinaryFormat, NodeContainerFormat>
    {
        private ParArchiveReaderParameters parameters = new ParArchiveReaderParameters
        {
            Recursive = false,
        };

        /// <summary>
        /// Initializes reader parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        public void Initialize(ParArchiveReaderParameters parameters)
        {
            this.parameters = parameters;
        }

        /// <inheritdoc/>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownserhip dispose transferred")]
        public NodeContainerFormat Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

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
            var folders = new Node[totalFolderCount];

            for (int i = 0; i < totalFolderCount; i++)
            {
                var folder = new ParFolder
                {
                    FolderCount = reader.ReadInt32(),
                    FirstFolderIndex = reader.ReadInt32(),
                    FileCount = reader.ReadInt32(),
                    FirstFileIndex = reader.ReadInt32(),
                    Unknown1 = reader.ReadInt32(),
                    Unknown2 = reader.ReadInt32(),
                    Unknown3 = reader.ReadInt32(),
                    Unknown4 = reader.ReadInt32(),
                };

                folders[i] = new Node(folderNames[i], folder);
            }

            reader.Stream.Seek(fileInfoOffset);
            var files = new Node[totalFileCount];
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

                var file = new ParFile(source.Stream, offset, compressedSize)
                {
                    IsCompressed = compressionFlag == 0x80000000,
                    DecompressedSize = size,
                    Unknown1 = unknown1,
                    Unknown2 = unknown2,
                    Unknown3 = unknown3,
                    Date = date,
                };

                files[i] = new Node(fileNames[i], file);
            }

            BuildTree(folders[0], folders, files, this.parameters.Recursive);

            var result = new NodeContainerFormat();
            result.Root.Add(folders[0].Children);
            return result;
        }

        private static void BuildTree(Node node, IReadOnlyList<Node> folders, IReadOnlyList<Node> files, bool recursive)
        {
            var nodeFormat = node.GetFormatAs<ParFolder>();

            for (int i = nodeFormat.FirstFolderIndex; i < nodeFormat.FirstFolderIndex + nodeFormat.FolderCount; i++)
            {
                node.Add(folders[i]);
                BuildTree(folders[i], folders, files, recursive);
            }

            for (int i = nodeFormat.FirstFileIndex; i < nodeFormat.FirstFileIndex + nodeFormat.FileCount; i++)
            {
                if (recursive && files[i].Name.EndsWith(".PAR", StringComparison.InvariantCultureIgnoreCase))
                {
                    files[i].TransformWith<ParArchiveReader>();
                }

                node.Add(files[i]);
            }
        }
    }
}
