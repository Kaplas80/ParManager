// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Uncompressor.cs" company="Kaplas80">
// © Kaplas80. Licensed under MIT. See LICENSE for details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace ParLib.Sllz
{
    using System;
    using System.IO;
    using System.Text;
    using Ionic.Zlib;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    /// <summary>
    /// Manages SLLZ compression used in Yakuza games.
    /// </summary>
    public class Uncompressor : IConverter<BinaryFormat, BinaryFormat>
    {
        /// <summary>Uncompresses a SLLZ format.</summary>
        /// <returns>The uncompressed format.</returns>
        /// <param name="source">Source format to convert.</param>
        public BinaryFormat Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            DataStream outputDataStream = Uncompress(source.Stream);

            return new BinaryFormat(outputDataStream);
        }

        private static DataStream Uncompress(DataStream inputDataStream)
        {
            var reader = new DataReader(inputDataStream)
            {
                DefaultEncoding = Encoding.ASCII,
            };

            inputDataStream.Seek(0, SeekMode.Start);

            string magic = reader.ReadString(4);

            if (magic != "SLLZ")
            {
                throw new FormatException("SLLZ: Bad magic Id.");
            }

            byte endianness = reader.ReadByte();
            reader.Endianness = endianness == 0 ? EndiannessMode.LittleEndian : EndiannessMode.BigEndian;
            byte version = reader.ReadByte();
            ushort headerSize = reader.ReadUInt16();

            int uncompressedSize = reader.ReadInt32();
            reader.ReadInt32(); // Compressed Size

            reader.Stream.Seek(headerSize, SeekMode.Start);

            if (version == 1)
            {
                return UncompressV1(inputDataStream, uncompressedSize);
            }

            if (version == 2)
            {
                return UncompressV2(inputDataStream, uncompressedSize);
            }

            throw new FormatException($"SLLZ: Unknown compression version {version}.");
        }

        private static DataStream UncompressV1(DataStream inputDataStream, int uncompressedSize)
        {
            var reader = new DataReader(inputDataStream);

            DataStream outputDataStream = DataStreamFactory.FromMemory();
            var writer = new DataWriter(outputDataStream);

            var flagReader = new FlagReader(inputDataStream);

            while (writer.Stream.Position < uncompressedSize)
            {
                int flag = flagReader.ReadFlag();

                if (flag == 0)
                {
                    byte data = reader.ReadByte();
                    writer.Write(data);
                }
                else
                {
                    (int offset, int size) = flagReader.GetCopyInfo();

                    writer.Stream.PushToPosition(-offset, SeekMode.Current);
                    var data = new byte[size];
                    writer.Stream.Read(data, 0, size);
                    writer.Stream.PopPosition();
                    writer.Write(data);
                }
            }

            if (uncompressedSize != outputDataStream.Length)
            {
                throw new FormatException("SLLZ: Wrong uncompressed data.");
            }

            return outputDataStream;
        }

        private static DataStream UncompressV2(DataStream inputDataStream, int uncompressedSize)
        {
            var reader = new DataReader(inputDataStream);

            DataStream outputDataStream = DataStreamFactory.FromMemory();
            var writer = new DataWriter(outputDataStream);

            // TODO: Check if endianness is always BigEndian
            reader.Endianness = EndiannessMode.BigEndian;

            int remaining = uncompressedSize;
            while (remaining != 0)
            {
                byte flag = reader.ReadByte();
                reader.Stream.Seek(-1, SeekMode.Current);

                int compressedChunkSize = (reader.ReadUInt16() << 8) | reader.ReadByte();
                int uncompressedChunkSize = reader.ReadUInt16() + 1;

                byte[] compressedData = reader.ReadBytes(compressedChunkSize - 5);

                if (flag >> 7 == 0)
                {
                    byte[] uncompressedData = ZlibDecompress(compressedData);

                    if (uncompressedChunkSize != uncompressedData.Length)
                    {
                        throw new FormatException("SLLZ: Wrong uncompressed data.");
                    }

                    writer.Write(uncompressedData);
                }
                else
                {
                    // I haven't found any file with this compression
                    // The code is in FUN_141e27350 (YakuzaKiwami2.exe v1.4)
                    throw new FormatException("SLLZ: Not ZLIB compression.");
                }

                remaining -= uncompressedChunkSize;

                reader.Stream.Seek(5, SeekMode.Current);
            }

            if (uncompressedSize != outputDataStream.Length)
            {
                throw new FormatException("SLLZ: Wrong uncompressed data.");
            }

            return outputDataStream;
        }

        private static byte[] ZlibDecompress(byte[] compressedData)
        {
            using var inputMemoryStream = new MemoryStream(compressedData);
            using var outputMemoryStream = new MemoryStream();
            using var zlibStream = new ZlibStream(outputMemoryStream, CompressionMode.Decompress);

            inputMemoryStream.CopyTo(zlibStream);

            return outputMemoryStream.ToArray();
        }

        private class FlagReader
        {
            private readonly DataReader reader;
            private byte currentValue;
            private byte bitCount;

            public FlagReader(DataStream inputDataStream)
            {
                this.reader = new DataReader(inputDataStream)
                {
                    Endianness = EndiannessMode.LittleEndian,
                };
                this.currentValue = this.reader.ReadByte();
                this.bitCount = 0x08;
            }

            public int ReadFlag()
            {
                int result = this.currentValue & 0x80;
                this.bitCount--;
                this.currentValue <<= 0x01;

                if (this.bitCount == 0x00)
                {
                    this.currentValue = this.reader.ReadByte();
                    this.bitCount = 0x08;
                }

                return result;
            }

            public Tuple<int, int> GetCopyInfo()
            {
                ushort copyFlags = this.reader.ReadUInt16(); // No se si la lectura es siempre en LittleEndian

                int offset = 1 + (copyFlags >> 4);
                int size = 3 + (copyFlags & 0xF);

                return new Tuple<int, int>(offset, size);
            }
        }
    }
}
