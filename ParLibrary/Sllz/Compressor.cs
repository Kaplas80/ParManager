// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParLibrary.Sllz
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
    public class Compressor : IConverter<ParFile, ParFile>, IInitializer<CompressorParameters>
    {
        private const int SearchSize = 4096;
        private const int MaxLength = 18;

        private CompressorParameters compressorParameters;

        /// <summary>
        /// Initializes the compressor parameters.
        /// </summary>
        /// <param name="parameters">Compressor configuration.</param>
        public void Initialize(CompressorParameters parameters)
        {
            this.compressorParameters = parameters;
        }

        /// <summary>Compresses a file with SLLZ.</summary>
        /// <returns>The compressed file.</returns>
        /// <param name="source">Source file to compress.</param>
        public ParFile Convert(ParFile source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.Stream.Seek(0, SeekMode.Start);

            DataStream outputDataStream = Compress(source.Stream, this.compressorParameters);

            var result = new ParFile(outputDataStream)
            {
                CanBeCompressed = false,
                IsCompressed = true,
                DecompressedSize = source.DecompressedSize,
                Attributes = source.Attributes,
                Unknown2 = source.Unknown2,
                Unknown3 = source.Unknown3,
                Date = source.Date,
            };

            return result;
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
            var inputData = new byte[inputDataStream.Length];
            var outputData = new byte[inputData.Length * 2];
            inputDataStream.Read(inputData, 0, inputData.Length);

            int inputPosition = 0;
            int outputPosition = 0;
            byte currentFlag = 0x00;
            int bitCount = 0;
            long flagPosition = outputPosition;

            outputData[flagPosition] = 0x00;
            outputPosition++;

            while (inputPosition < inputData.Length)
            {
                Tuple<int, int> match = FindMatch(inputData, inputPosition);

                if (match == null)
                {
                    // currentFlag |= (byte)(0 << (7 - bitCount)); // It's zero
                    bitCount++;

                    if (bitCount == 0x08)
                    {
                        outputData[flagPosition] = currentFlag;

                        currentFlag = 0x00;
                        bitCount = 0x00;
                        flagPosition = outputPosition;
                        outputData[flagPosition] = 0x00;
                        outputPosition++;
                    }

                    outputData[outputPosition] = inputData[inputPosition];
                    inputPosition++;
                    outputPosition++;
                }
                else
                {
                    currentFlag |= (byte)(1 << (7 - bitCount));
                    bitCount++;

                    if (bitCount == 0x08)
                    {
                        outputData[flagPosition] = currentFlag;

                        currentFlag = 0x00;
                        bitCount = 0x00;
                        flagPosition = outputPosition;
                        outputData[flagPosition] = 0x00;
                        outputPosition++;
                    }

                    short offset = (short)((match.Item1 - 1) << 4);
                    short size = (short)((match.Item2 - 3) & 0x0F);

                    short tuple = (short)(offset | size);

                    outputData[outputPosition] = (byte)tuple;
                    outputPosition++;
                    outputData[outputPosition] = (byte)(tuple >> 8);
                    outputPosition++;

                    inputPosition += match.Item2;
                }
            }

            outputData[flagPosition] = currentFlag;

            DataStream outputDataStream = DataStreamFactory.FromArray(outputData, 0, outputPosition);
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
                int decompressedChunkSize = Math.Min(input.Length - currentPosition, 0x10000);
                var decompressedData = new byte[decompressedChunkSize];
                Array.Copy(input, currentPosition, decompressedData, 0, decompressedChunkSize);

                byte[] compressedData = ZlibCompress(decompressedData);

                int compressedDataLength = compressedData.Length + 5;
                writer.Write((byte)(compressedDataLength >> 16));
                writer.Write((byte)(compressedDataLength >> 8));
                writer.Write((byte)compressedDataLength);
                writer.Write((ushort)(decompressedChunkSize - 1));
                writer.Write(compressedData);
                writer.WriteTimes(0, 5);

                currentPosition += decompressedChunkSize;
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
            int length = 1;

            while (length < maxLength && input[pos1] == input[pos2])
            {
                pos1++;
                pos2++;
                length++;
            }

            return length;
        }

        private static byte[] ZlibCompress(byte[] decompressedData)
        {
            using (var inputMemoryStream = new MemoryStream(decompressedData))
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
