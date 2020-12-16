// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Converters
{
    using System;
    using System.Text;
    using K4os.Compression.LZ4;
    using SakunaTools.Types;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    /// <summary>
    /// ARC container compressor.
    /// </summary>
    public class ArcCompress : IConverter<BinaryFormat, BinaryFormat>
    {
        /// <summary>
        /// Compresses an ARC container using LZ4.
        /// </summary>
        /// <param name="source">Source format.</param>
        /// <returns>The compressed format.</returns>
        public BinaryFormat Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.Stream.Position = 0;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var reader = new DataReader(source.Stream)
            {
                DefaultEncoding = Encoding.GetEncoding(1252),
                Endianness = EndiannessMode.LittleEndian,
            };

            // Read the file header
            ArcHeader header = reader.Read<ArcHeader>() as ArcHeader;
            this.CheckHeader(header);

            switch (header.CompressionType)
            {
                case 0x00: // Not compressed
                {
                    DataStream outputStream = DataStreamFactory.FromMemory();
                    var writer = new DataWriter(outputStream)
                    {
                        DefaultEncoding = Encoding.GetEncoding(1252),
                        Endianness = EndiannessMode.LittleEndian,
                    };

                    header.CompressionType = 0x02;
                    writer.WriteOfType(header);
                    byte[] filesInfo = reader.ReadBytes(0x70 * header.FileCount);
                    byte[] decompressedData = reader.ReadBytes((int)(source.Stream.Length - source.Stream.Position));

                    var compressedData = new byte[LZ4Codec.MaximumOutputSize(decompressedData.Length)];
                    int bytesWritten = LZ4Codec.Encode(decompressedData, compressedData, LZ4Level.L11_OPT);
                    if (bytesWritten < 0)
                    {
                        throw new FormatException($"ARC: Error in LZ4 compression.");
                    }

                    writer.Write(filesInfo);

                    var data = new byte[bytesWritten];
                    Array.Copy(compressedData, data, bytesWritten);
                    writer.Write(data);

                    return new BinaryFormat(outputStream);
                }

                case 0x02: // LZ4 Compression
                    // Already compressed
                    return source;

                default:
                    throw new FormatException($"ARC: Unknown compression {header.CompressionType:X4}");
            }
        }

        private void CheckHeader(ArcHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (header.MagicId != "TGP0")
            {
                throw new FormatException($"ARC: Bad magic Id ({header.MagicId} != TGP0)");
            }
        }
    }
}