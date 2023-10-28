// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParLibrary.Sllz
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
    public class Decompressor : IConverter<ParFile, ParFile>
    {
        /// <summary>Decompresses a SLLZ file.</summary>
        /// <returns>The decompressed file.</returns>
        /// <param name="source">Source file to decompress.</param>
        public ParFile Convert(ParFile source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.Stream.Position = 0;

            DataStream outputDataStream = Decompress(source.Stream);

            var result = new ParFile(outputDataStream)
            {
                CanBeCompressed = true,
                IsCompressed = false,
                DecompressedSize = source.DecompressedSize,
                Attributes = source.Attributes,
                Timestamp = source.Timestamp,
            };

            return result;
        }

        private static DataStream Decompress(DataStream inputDataStream)
        {
            var reader = new DataReader(inputDataStream)
            {
                DefaultEncoding = Encoding.ASCII,
            };

            inputDataStream.Seek(0);

            string magic = reader.ReadString(4);

            if (magic != "SLLZ")
            {
                throw new FormatException("SLLZ: Bad magic Id.");
            }

            byte endianness = reader.ReadByte();
            reader.Endianness = endianness == 0 ? EndiannessMode.LittleEndian : EndiannessMode.BigEndian;
            byte version = reader.ReadByte();
            ushort headerSize = reader.ReadUInt16();

            int decompressedSize = reader.ReadInt32();
            int compressedSize = reader.ReadInt32();

            reader.Stream.Seek(headerSize, SeekOrigin.Begin);

            if (version == 1)
            {
                return DecompressV1(inputDataStream, compressedSize, decompressedSize);
            }

            if (version == 2)
            {
                return DecompressV2(inputDataStream, compressedSize, decompressedSize);
            }

            throw new FormatException($"SLLZ: Unknown compression version {version}.");
        }

        private static DataStream DecompressV1(DataStream inputDataStream, int compressedSize, int decompressedSize)
        {
            var inputData = new byte[compressedSize];
            var outputData = new byte[decompressedSize];
            inputDataStream.Read(inputData, 0, compressedSize - 0x10);

            var inputPosition = 0;
            var outputPosition = 0;

            byte flag = inputData[inputPosition];
            inputPosition++;
            var flagCount = 8;

            do
            {
                if ((flag & 0x80) == 0x80)
                {
                    flag = (byte)(flag << 1);
                    flagCount--;
                    if (flagCount == 0)
                    {
                        flag = inputData[inputPosition];
                        inputPosition++;
                        flagCount = 8;
                    }

                    var copyFlags = (ushort)(inputData[inputPosition] | inputData[inputPosition + 1] << 8);
                    inputPosition += 2;

                    int copyDistance = 1 + (copyFlags >> 4);
                    int copyCount = 3 + (copyFlags & 0xF);

                    var i = 0;
                    do
                    {
                        outputData[outputPosition] = outputData[outputPosition - copyDistance];
                        outputPosition++;
                        i++;
                    }
                    while (i < copyCount);
                }
                else
                {
                    flag = (byte)(flag << 1);
                    flagCount--;
                    if (flagCount == 0)
                    {
                        flag = inputData[inputPosition];
                        inputPosition++;
                        flagCount = 8;
                    }

                    outputData[outputPosition] = inputData[inputPosition];
                    inputPosition++;
                    outputPosition++;
                }
            }
            while (outputPosition < decompressedSize);

            DataStream outputDataStream = DataStreamFactory.FromArray(outputData, 0, decompressedSize);
            return outputDataStream;
        }

        private static DataStream DecompressV2(DataStream inputDataStream, int compressedSize, int decompressedSize)
        {
            var inputData = new byte[compressedSize];
            var outputData = new byte[decompressedSize];
            inputDataStream.Read(inputData, 0, compressedSize - 0x10);

            var inputPosition = 0;
            var outputPosition = 0;

            while (outputPosition < decompressedSize)
            {
                int compressedChunkSize = (inputData[inputPosition] << 16) | (inputData[inputPosition + 1] << 8) | inputData[inputPosition + 2];
                int decompressedChunkSize = ((inputData[inputPosition + 3] << 8) | inputData[inputPosition + 4]) + 1;

                bool isCompressed = (compressedChunkSize & 0x00800000) == 0x00000000;

                if (isCompressed)
                {
                    byte[] decompressedData = ZlibDecompress(inputData, inputPosition + 5, compressedChunkSize - 5);

                    if (decompressedChunkSize != decompressedData.Length)
                    {
                        throw new FormatException("SLLZ: Wrong decompressed data.");
                    }

                    Array.Copy(decompressedData, 0, outputData, outputPosition, decompressedData.Length);
                    inputPosition += compressedChunkSize;
                }
                else
                {
                    // The data isn't compressed in this chunk, just copy it
                    compressedChunkSize = (int)(compressedChunkSize & 0xFF7FFFFF);
                    Array.Copy(inputData, inputPosition + 5, outputData, outputPosition, decompressedChunkSize);
                    inputPosition += compressedChunkSize;
                }

                outputPosition += decompressedChunkSize;
            }

            DataStream outputDataStream = DataStreamFactory.FromArray(outputData, 0, decompressedSize);
            return outputDataStream;
        }

        private static byte[] ZlibDecompress(byte[] compressedData, int index, int count)
        {
            using (var inputMemoryStream = new MemoryStream(compressedData, index, count))
            using (var outputMemoryStream = new MemoryStream())
            using (var zlibStream = new ZlibStream(outputMemoryStream, CompressionMode.Decompress))
            {
                inputMemoryStream.CopyTo(zlibStream);

                return outputMemoryStream.ToArray();
            }
        }
    }
}
