// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Converters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    /// <summary>
    /// Converts a Nhtex file into a DDS file.
    /// <remarks>Only font nhtex.</remarks>
    /// </summary>
    public class NhtexToDds : IConverter<BinaryFormat, BinaryFormat>
    {
        private static readonly byte[] DdsHeader =
        {
            0x44, 0x44, 0x53, 0x20, 0x7C, 0x00, 0x00, 0x00, 0x07, 0x10, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00, 0x42, 0x43, 0x34, 0x55, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };

        /// <summary>
        /// Convert the nhtex into a dds.
        /// </summary>
        /// <param name="source">The source format.</param>
        /// <returns>The dds.</returns>
        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "Ownserhip dispose transferred")]
        public BinaryFormat Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.Stream.Position = 0;
            var reader = new DataReader(source.Stream)
            {
                DefaultEncoding = Encoding.ASCII,
                Endianness = EndiannessMode.LittleEndian,
            };

            source.Stream.Seek(0x18, System.IO.SeekOrigin.Begin);
            long dataSize = reader.ReadInt64();
            source.Stream.Seek(0x40, System.IO.SeekOrigin.Begin);
            uint width = reader.ReadUInt32();
            uint height = reader.ReadUInt32();
            source.Stream.Seek(0x70, System.IO.SeekOrigin.Begin);
            uint numMipmaps = reader.ReadUInt32();
            source.Stream.Seek(0x80, System.IO.SeekOrigin.Begin);
            long dataOffset = reader.ReadInt64() + 0x80;
            source.Stream.Seek(dataOffset, System.IO.SeekOrigin.Begin);
            byte[] data = reader.ReadBytes((int)(dataSize - dataOffset + 0x30));

            DataStream outputDataStream = DataStreamFactory.FromMemory();
            var writer = new DataWriter(outputDataStream)
            {
                DefaultEncoding = Encoding.ASCII,
                Endianness = EndiannessMode.LittleEndian,
            };

            writer.Write(DdsHeader);
            writer.Write(data);

            writer.Stream.Seek(0x0C, System.IO.SeekOrigin.Begin);
            writer.Write(height);
            writer.Write(width);
            writer.Write(height / 4 * width / 4 * 8);
            writer.Stream.Seek(0x1C, System.IO.SeekOrigin.Begin);
            writer.Write(numMipmaps);

            return new BinaryFormat(outputDataStream);
        }
    }
}