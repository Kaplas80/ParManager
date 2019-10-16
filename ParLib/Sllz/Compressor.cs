// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Compressor.cs" company="Kaplas80">
// © Kaplas80. Licensed under MIT. See LICENSE for details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace ParLib.Sllz
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Ionic.Zlib;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    /// <summary>
    /// Manages SLLZ compression used in Yakuza games.
    /// </summary>
    public class Compressor : IConverter<BinaryFormat, BinaryFormat>, IInitializer<CompressorParameters>
    {
        private const int SearchSize = 4096;
        private const int MaxLength = 18;

        private CompressorParameters parameters;

        /// <summary>
        /// Initializes the compressor parameters.
        /// </summary>
        /// <param name="parameters">Compressor configuration.</param>
        public void Initialize(CompressorParameters parameters)
        {
            this.parameters = parameters;
        }

        /// <summary>Compresses a SLLZ format.</summary>
        /// <returns>The compressed format.</returns>
        /// <param name="source">Source format to convert.</param>
        public BinaryFormat Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            DataStream outputDataStream = Compress(source.Stream, this.parameters);

            return new BinaryFormat(outputDataStream);
        }

        private static DataStream Compress(DataStream inputDataStream, CompressorParameters parameters)
        {
            DataStream outputDataStream = DataStreamFactory.FromMemory();
            var writer = new DataWriter(outputDataStream)
            {
                DefaultEncoding = Encoding.ASCII,
            };

            if (parameters == null)
            {
                parameters = new CompressorParameters
                {
                    Version = 0x01,
                    Endianness = 0x00,
                };
            }

            writer.Endianness = parameters.Endianness == 0 ? EndiannessMode.LittleEndian : EndiannessMode.BigEndian;
            writer.Write("SLLZ", false);
            writer.Write(parameters.Endianness);
            writer.Write(parameters.Version);
            writer.Write((ushort)0x10); // Header size
            writer.Write((int)inputDataStream.Length);
            writer.Stream.PushCurrentPosition();
            writer.Write(0x00000000); // Compressed size

            DataStream compressedDataStream;
            if (parameters.Version == 1)
            {
                compressedDataStream = CompressV1(inputDataStream);
            }
            else if (parameters.Version == 2)
            {
                if (inputDataStream.Length < 0x1B)
                {
                    throw new FormatException($"SLLZv2: Input size must more than 0x1A.");
                }

                compressedDataStream = CompressV2(inputDataStream);
            }
            else
            {
                throw new FormatException($"SLLZ: Unknown compression version {parameters.Version}.");
            }

            compressedDataStream.WriteTo(outputDataStream);
            writer.Stream.PopPosition();
            writer.Write((int)(compressedDataStream.Length + 0x10)); // data + header

            compressedDataStream.Dispose();

            return outputDataStream;
        }

        private static DataStream CompressV1(DataStream inputDataStream)
        {
            // It's easier to implement working with a byte array.
            var input = new byte[inputDataStream.Length];
            inputDataStream.Read(input, 0, input.Length);

            DataStream outputDataStream = DataStreamFactory.FromMemory();
            var writer = new DataWriter(outputDataStream);

            int currentPosition = 0;
            byte currentFlag = 0x00;
            int bitCount = 0;
            long flagPosition = outputDataStream.Position;
            writer.Write((byte)0x00);

            while (currentPosition < input.Length)
            {
                Tuple<int, int> match = FindMatch(input, currentPosition);

                if (match == null)
                {
                    // currentFlag |= (byte)(0 << (7 - bitCount)); // It's zero
                    bitCount++;

                    if (bitCount == 0x08)
                    {
                        outputDataStream.PushToPosition(flagPosition);
                        writer.Write(currentFlag);
                        outputDataStream.PopPosition();

                        currentFlag = 0x00;
                        bitCount = 0x00;
                        flagPosition = outputDataStream.Position;
                        writer.Write((byte)0x00);
                    }

                    writer.Write(input[currentPosition]);
                    currentPosition++;
                }
                else
                {
                    currentFlag |= (byte)(1 << (7 - bitCount));
                    bitCount++;

                    if (bitCount == 0x08)
                    {
                        outputDataStream.PushToPosition(flagPosition);
                        writer.Write(currentFlag);
                        outputDataStream.PopPosition();

                        currentFlag = 0x00;
                        bitCount = 0x00;
                        flagPosition = outputDataStream.Position;
                        writer.Write((byte)0x00);
                    }

                    var offset = (short)((match.Item1 - 1) << 4);
                    var size = (short)((match.Item2 - 3) & 0x0F);

                    var tuple = (short)(offset | size);

                    writer.Write(tuple);

                    currentPosition += match.Item2;
                }
            }

            outputDataStream.PushToPosition(flagPosition);
            writer.Write(currentFlag);
            outputDataStream.PopPosition();

            return outputDataStream;
        }

        private static DataStream CompressV2(DataStream inputDataStream)
        {
            var input = new byte[inputDataStream.Length];
            inputDataStream.Read(input, 0, input.Length);

            DataStream outputDataStream = DataStreamFactory.FromMemory();
            var writer = new DataWriter(outputDataStream)
            {
                Endianness = EndiannessMode.BigEndian,
            };

            int currentPosition = 0;

            while (currentPosition < input.Length)
            {
                int uncompressedChunkSize = Math.Min(input.Length - currentPosition, 0x10000);
                var uncompressedData = new byte[uncompressedChunkSize];
                Array.Copy(input, currentPosition, uncompressedData, 0, uncompressedChunkSize);

                byte[] compressedData = ZlibCompress(uncompressedData);

                int compressedDataLength = compressedData.Length + 5;
                writer.Write((byte)(compressedDataLength >> 16));
                writer.Write((byte)(compressedDataLength >> 8));
                writer.Write((byte)compressedDataLength);
                writer.Write((ushort)(uncompressedChunkSize - 1));
                writer.Write(compressedData);
                writer.WriteTimes(0, 5);

                currentPosition += uncompressedChunkSize;
            }

            return outputDataStream;
        }

        private static Tuple<int, int> FindMatch(IReadOnlyList<byte> inputData, int readPosition)
        {
            int bestPosition = 0;
            int bestLength = 1;

            int current = readPosition - 1;

            int startPos = Math.Max(readPosition - SearchSize, 0);

            while (current >= startPos)
            {
                if (inputData[current] == inputData[readPosition])
                {
                    int maxLength = Math.Min(inputData.Count - readPosition, MaxLength);
                    maxLength = Math.Min(maxLength, readPosition - current);
                    int length = DataCompare(inputData, current + 1, readPosition + 1, maxLength);
                    if (length > bestLength)
                    {
                        bestLength = length;
                        bestPosition = current;
                    }
                }

                current--;
            }

            return bestLength >= 3 ? new Tuple<int, int>(readPosition - bestPosition, bestLength) : null;
        }

        private static int DataCompare(IReadOnlyList<byte> input, int pos1, int pos2, int maxLength)
        {
            var length = 1;

            while (length < maxLength && input[pos1] == input[pos2])
            {
                pos1++;
                pos2++;
                length++;
            }

            return length;
        }

        private static byte[] ZlibCompress(byte[] uncompressedData)
        {
            using (var inputMemoryStream = new MemoryStream(uncompressedData))
            using (var outputMemoryStream = new MemoryStream())
            using (var zlibStream = new ZlibStream(outputMemoryStream, CompressionMode.Compress, CompressionLevel.BestCompression))
            {
                inputMemoryStream.CopyTo(zlibStream);
                zlibStream.Close();

                return outputMemoryStream.ToArray();
            }
        }
    }
}
