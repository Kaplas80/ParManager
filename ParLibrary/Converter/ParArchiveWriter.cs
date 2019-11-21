// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParLibrary.Converter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using ParLibrary.Sllz;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Converter from PAR to BinaryFormat.
    /// </summary>
    public class ParArchiveWriter : IInitializer<ParArchiveWriterParameters>, IConverter<NodeContainerFormat, ParFile>
    {
        private ParArchiveWriterParameters parameters = new ParArchiveWriterParameters
        {
            CompressorVersion = 0x01,
        };

        /// <inheritdoc />
        public void Initialize(ParArchiveWriterParameters parameters)
        {
            this.parameters = parameters;
        }

        /// <summary>
        /// Converts a PAR format into binary.
        /// </summary>
        /// <param name="source">The par.</param>
        /// <returns>The BinaryFormat.</returns>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownserhip dispose transferred")]
        public ParFile Convert(NodeContainerFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            DataStream dataStream = DataStreamFactory.FromMemory();

            var writer = new DataWriter(dataStream)
            {
                DefaultEncoding = Encoding.GetEncoding(1252),
                Endianness = EndiannessMode.BigEndian,
            };

            var folders = new List<Node>();
            var files = new List<Node>();

            var parFolderRootNode = new Node(".", new NodeContainerFormat());
            source.MoveChildrenTo(parFolderRootNode);

            folders.Add(parFolderRootNode);

            GetFoldersAndFiles(source.Root, folders, files);
            CompressFiles(files, this.parameters.CompressorVersion);

            int headerSize = 32 + (64 * folders.Count) + (64 * files.Count);
            int folderTableOffset = headerSize;
            int fileTableOffset = folderTableOffset + (folders.Count * 32);
            long dataPosition = fileTableOffset + (files.Count * 32);
            dataPosition = Align(dataPosition, 2048);

            writer.Write("PARC", 4, false);
            writer.Write(0x02010000);
            writer.Write(0x00020001);
            writer.Write(0x00000000);
            writer.Write(folders.Count);
            writer.Write(folderTableOffset);
            writer.Write(files.Count);
            writer.Write(fileTableOffset);

            WriteNames(writer, folders);
            WriteNames(writer, files);

            WriteFolders(writer, folders);
            WriteFiles(writer, files, dataPosition);

            dataStream.Seek(0, SeekMode.End);
            writer.WritePadding(0, 2048);

            var result = new ParFile(dataStream)
            {
                CanBeCompressed = false,
            };

            return result;
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownserhip dispose transferred")]
        private static void GetFoldersAndFiles(Node root, ICollection<Node> folders, ICollection<Node> files)
        {
            int folderIndex = 1;
            int fileIndex = 0;

            var queue = new Queue<Node>();

            queue.Enqueue(root);

            while (queue.Count != 0)
            {
                Node folder = queue.Dequeue();

                folder.Tags["FirstFolderIndex"] = folderIndex;
                folder.Tags["FolderCount"] = 0;
                folder.Tags["FirstFileIndex"] = fileIndex;
                folder.Tags["FileCount"] = 0;
                folder.Tags["Attributes"] = 0x00000010;
                folder.Tags["Unknown2"] = 0x00000000;
                folder.Tags["Unknown3"] = 0x00000000;
                folder.Tags["Unknown4"] = 0x00000000;

                foreach (Node child in folder.Children)
                {
                    if (child.IsContainer)
                    {
                        if (child.Name.EndsWith(".PAR", StringComparison.InvariantCultureIgnoreCase))
                        {
                            child.TransformWith<ParArchiveWriter>();

                            files.Add(child);
                            fileIndex++;
                            folder.Tags["FileCount"]++;
                        }
                        else
                        {
                            folders.Add(child);
                            folderIndex++;
                            folder.Tags["FolderCount"]++;

                            queue.Enqueue(child);
                        }
                    }
                    else
                    {
                        child.TransformWith<ParFile>();
                        files.Add(child);
                        fileIndex++;
                        folder.Tags["FileCount"]++;
                    }
                }
            }
        }

        private static void CompressFiles(IEnumerable<Node> files, int compressorVersion)
        {
            var compressorParameters = new CompressorParameters
            {
                Endianness = 0x00,
                Version = (byte)compressorVersion,
            };

            Parallel.ForEach(files, node =>
            {
                var parFile = node.GetFormatAs<ParFile>();
                if (parFile == null)
                {
                    return;
                }

                if (parFile.CanBeCompressed)
                {
                    node.TransformWith<Compressor, CompressorParameters>(compressorParameters);
                }
            });
        }

        private static long Align(long position, int align)
        {
            if (position % align == 0)
            {
                return position;
            }

            long padding = align + (-position % align);
            return position + padding;
        }

        private static void WriteNames(DataWriter writer, IEnumerable<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                writer.Write(node.Name, 64, false);
            }
        }

        private static void WriteFolders(DataWriter writer, IEnumerable<Node> folders)
        {
            foreach (Node node in folders)
            {
                int attributes = 0x00000010;
                if (node.Tags.ContainsKey("DirectoryInfo"))
                {
                    FileInfo info = node.Tags["DirectoryInfo"];
                    attributes = (int)info.Attributes;
                }

                if (node.Tags.ContainsKey("Attributes"))
                {
                    attributes = (int)node.Tags["Attributes"];
                }

                writer.Write((int)node.Tags["FolderCount"]);
                writer.Write((int)node.Tags["FirstFolderIndex"]);
                writer.Write((int)node.Tags["FileCount"]);
                writer.Write((int)node.Tags["FirstFileIndex"]);
                writer.Write(attributes);
                writer.Write((int)node.Tags["Unknown2"]);
                writer.Write((int)node.Tags["Unknown3"]);
                writer.Write((int)node.Tags["Unknown4"]);
            }
        }

        private static void WriteFiles(DataWriter writer, IEnumerable<Node> files, long dataPosition)
        {
            long blockSize = 0;

            foreach (Node node in files)
            {
                var parFile = node.GetFormatAs<ParFile>();
                if (parFile == null)
                {
                    continue;
                }

                if (node.Stream.Length > 2048)
                {
                    blockSize = 2048 + (-node.Stream.Length % 2048);
                    dataPosition = Align(dataPosition, 2048);
                }
                else
                {
                    if (node.Stream.Length < blockSize)
                    {
                        blockSize -= node.Stream.Length;
                    }
                    else
                    {
                        blockSize = 2048 + (-node.Stream.Length % 2048);
                        dataPosition = Align(dataPosition, 2048);
                    }
                }

                int attributes = parFile.Attributes;
                DateTime date = parFile.FileDate;

                if (node.Tags.ContainsKey("FileInfo"))
                {
                    FileInfo info = node.Tags["FileInfo"];
                    attributes = (int)info.Attributes;
                    date = info.LastWriteTime;
                }

                var baseDate = new DateTime(1970, 1, 1);
                int seconds = (int)(date - baseDate).TotalSeconds;

                writer.Write(parFile.IsCompressed ? 0x80000000 : 0x00000000);
                writer.Write(parFile.DecompressedSize);
                writer.Write((int)node.Stream.Length);
                writer.Write((int)dataPosition);
                writer.Write(attributes);
                writer.Write(parFile.Unknown2);
                writer.Write(parFile.Unknown3);
                writer.Write(seconds);

                writer.Stream.PushToPosition(0, SeekMode.End);
                writer.WriteUntilLength(0, dataPosition);
                node.Stream.WriteTo(writer.Stream);
                dataPosition = writer.Stream.Position;
                writer.Stream.PopPosition();
            }
        }
    }
}
