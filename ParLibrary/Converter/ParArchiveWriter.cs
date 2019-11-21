// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParLibrary.Converter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
            OutputFile = string.Empty,
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

            DataStream dataStream = string.IsNullOrEmpty(this.parameters.OutputFile)
                ? DataStreamFactory.FromMemory()
                : DataStreamFactory.FromFile(this.parameters.OutputFile, FileOpenMode.Write);

            var writer = new DataWriter(dataStream)
            {
                DefaultEncoding = Encoding.GetEncoding(1252),
                Endianness = EndiannessMode.BigEndian,
            };

            var folders = new List<Node>();
            var files = new List<Node>();

            var parFolderRoot = new ParFolder();
            var parFolderRootNode = new Node(".", parFolderRoot);
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
                var parFolder = folder.GetFormatAs<ParFolder>();
                parFolder.FirstFolderIndex = folderIndex;
                parFolder.FirstFileIndex = fileIndex;

                foreach (Node child in folder.Children)
                {
                    if (child.IsContainer)
                    {
                        if (child.Name.EndsWith(".PAR", StringComparison.InvariantCultureIgnoreCase))
                        {
                            child.TransformWith<ParArchiveWriter>();

                            files.Add(child);
                            fileIndex++;
                            parFolder.FileCount++;
                        }
                        else
                        {
                            folders.Add(child);
                            folderIndex++;
                            parFolder.FolderCount++;

                            queue.Enqueue(child);
                        }
                    }
                    else
                    {
                        files.Add(child);
                        fileIndex++;
                        parFolder.FileCount++;
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
                var parFolder = node.GetFormatAs<ParFolder>();
                if (parFolder == null)
                {
                    continue;
                }

                writer.Write(parFolder.FolderCount);
                writer.Write(parFolder.FirstFolderIndex);
                writer.Write(parFolder.FileCount);
                writer.Write(parFolder.FirstFileIndex);
                writer.Write(parFolder.Unknown1);
                writer.Write(parFolder.Unknown2);
                writer.Write(parFolder.Unknown3);
                writer.Write(parFolder.Unknown4);
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

                if (parFile.Stream.Length > 2048)
                {
                    blockSize = 2048 + (-parFile.Stream.Length % 2048);
                    dataPosition = Align(dataPosition, 2048);
                }
                else
                {
                    if (parFile.Stream.Length < blockSize)
                    {
                        blockSize -= parFile.Stream.Length;
                    }
                    else
                    {
                        blockSize = 2048 + (-parFile.Stream.Length % 2048);
                        dataPosition = Align(dataPosition, 2048);
                    }
                }

                writer.Write(parFile.IsCompressed ? 0x80000000 : 0x00000000);
                writer.Write(parFile.DecompressedSize);
                writer.Write((int)parFile.Stream.Length);
                writer.Write((int)dataPosition);
                writer.Write(parFile.Unknown1);
                writer.Write(parFile.Unknown2);
                writer.Write(parFile.Unknown3);
                writer.Write(parFile.Date);

                writer.Stream.PushToPosition(0, SeekMode.End);
                writer.WriteUntilLength(0, dataPosition);
                parFile.Stream.WriteTo(writer.Stream);
                dataPosition = writer.Stream.Position;
                writer.Stream.PopPosition();
            }
        }
    }
}
