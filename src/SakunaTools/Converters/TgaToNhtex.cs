// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using BCnEncoder.Encoder;
    using BCnEncoder.Shared;
    using SixLabors.ImageSharp;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    /// <summary>
    /// Converts a TGA into a nhtex.
    /// </summary>
    public class TgaToNhtex : IConverter<BinaryFormat, BinaryFormat>
    {
        /// <summary>
        /// Converts a TGA into a nhtex.
        /// </summary>
        /// <param name="source">The source format.</param>
        /// <returns>The nhtex.</returns>
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

            byte[] tgaData = reader.ReadBytes((int)source.Stream.Length);
            using var image = Image.Load(tgaData);

            var encoder = new BcEncoder
            {
                OutputOptions =
                {
                    generateMipMaps = true,
                    quality = CompressionQuality.BestQuality,
                    format = CompressionFormat.BC4,
                    fileFormat = OutputFileFormat.Dds,
                },
            };

            List<byte[]> mipmaps = encoder.EncodeToRawBytes(image);

            return new BinaryFormat(this.Write((uint)image.Height, (uint)image.Width, mipmaps));
        }

        /// <summary>
        /// Write the mipmaps to a DataStream.
        /// </summary>
        /// <param name="height">Image height (in pixels).</param>
        /// <param name="width">Image width (in pixels).</param>
        /// <param name="mipmaps">The list of mipmaps.</param>
        /// <returns>The DataStream.</returns>
        protected virtual DataStream Write(uint height, uint width, List<byte[]> mipmaps)
        {
            if (mipmaps == null)
            {
                throw new ArgumentNullException(nameof(mipmaps));
            }

            DataStream outputDataStream = DataStreamFactory.FromMemory();
            var writer = new DataWriter(outputDataStream)
            {
                DefaultEncoding = Encoding.ASCII,
                Endianness = EndiannessMode.LittleEndian,
            };

            writer.Write(0x20L);
            writer.Write(0x10L);
            writer.Write(0x20L);
            writer.Write(0x00L); // Size
            writer.Write(0x00L);
            writer.Write(0x00L);

            writer.Write(0x50);
            writer.Write(0x00);
            writer.Write(0x00);
            writer.Write(0x03);

            writer.Write(width);
            writer.Write(height);
            writer.Write(0x01);
            writer.Write(0x01);

            writer.Write((long)mipmaps.Count);
            writer.Write(0x00L);
            writer.Write(0x00L);
            writer.Write(0x10L);

            writer.Write((long)mipmaps.Count);
            long baseOffset = writer.Stream.Position;
            long indexSize = 0x18 * mipmaps.Count;
            long acum = indexSize;
            for (var i = 0; i < mipmaps.Count; i++)
            {
                uint pitch = Math.Max(1, ((width >> i) + 3) / 4) * 8;
                writer.Write(pitch);
                writer.Write(mipmaps[i].Length);
                writer.Write(acum - (writer.Stream.Position - baseOffset));
                writer.Write((long)mipmaps[i].Length);

                acum += mipmaps[i].Length;
            }

            foreach (byte[] mipmap in mipmaps)
            {
                writer.Write(mipmap);
            }

            writer.Stream.Seek(0x18, SeekMode.Start);
            writer.Write(outputDataStream.Length - 0x30);

            return outputDataStream;
        }
    }
}