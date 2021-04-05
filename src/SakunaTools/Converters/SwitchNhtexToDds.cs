// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using SakunaTools.Types;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    /// <summary>
    /// Converts a Nhtex file into a DDS file.
    /// <remarks>Only font nhtex.</remarks>
    /// </summary>
    public class SwitchNhtexToDds : IConverter<BinaryFormat, BinaryFormat>
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

            source.Stream.Seek(0x48, System.IO.SeekOrigin.Begin);
            long dataSize = reader.ReadInt64();
            source.Stream.Seek(0x50, System.IO.SeekOrigin.Begin);
            uint width = reader.ReadUInt32();
            uint height = reader.ReadUInt32();
            source.Stream.Seek(0x64, System.IO.SeekOrigin.Begin);
            uint numMipmaps = reader.ReadUInt32();
            source.Stream.Seek(0x40, System.IO.SeekOrigin.Begin);
            long dataOffset = reader.ReadInt64() + 0x40;
            source.Stream.Seek(dataOffset, System.IO.SeekOrigin.Begin);
            byte[] data = reader.ReadBytes((int)dataSize);

            DataStream outputDataStream = DataStreamFactory.FromMemory();
            var writer = new DataWriter(outputDataStream)
            {
                DefaultEncoding = Encoding.ASCII,
                Endianness = EndiannessMode.LittleEndian,
            };

            writer.Write(DdsHeader);
            writer.Write(this.Deswizzle(width, height, numMipmaps, data));

            writer.Stream.Seek(0x0C, System.IO.SeekOrigin.Begin);
            writer.Write(height);
            writer.Write(width);
            writer.Write(height / 4 * width / 4 * 8);
            writer.Stream.Seek(0x1C, System.IO.SeekOrigin.Begin);
            writer.Write(numMipmaps);

            return new BinaryFormat(outputDataStream);
        }

        private byte[] Deswizzle(uint width, uint height, uint numMipmaps, byte[] image)
        {
            uint bpp = 8;
            uint blkWidth = 4;
            uint blkHeight = 4;
            uint dataAlignment = 512;

            uint blockHeight = SwitchSwizzler.GetBlockHeight(SwitchSwizzler.DivRoundUp(height, blkHeight));
            uint blockHeightLog2 = (uint)System.Convert.ToString(blockHeight, 2).Length - 1;
            int linesPerBlockHeight = (1 << (int)blockHeightLog2) * 8;

            uint totalSize = 0;
            for (var i = 0; i < numMipmaps; i++)
            {
                uint mipmapWidth = Math.Max(1, width >> i);
                uint mipmapHeight = Math.Max(1, height >> i);
                uint mipmapSize = SwitchSwizzler.DivRoundUp(mipmapWidth, blkWidth) *
                                  SwitchSwizzler.DivRoundUp(mipmapHeight, blkHeight) *
                                  bpp;

                totalSize += mipmapSize;
            }

            var result = new byte[totalSize];

            uint surfaceSize = 0;
            var blockHeightShift = 0;

            List<uint> mipOffsets = new List<uint>();

            uint resultOffset = 0;
            for (var i = 0; i < numMipmaps; i++)
            {
                uint mipmapWidth = Math.Max(1, width >> i);
                uint mipmapHeight = Math.Max(1, height >> i);
                uint mipmapSize = SwitchSwizzler.DivRoundUp(mipmapWidth, blkWidth) *
                                  SwitchSwizzler.DivRoundUp(mipmapHeight, blkHeight) *
                                  bpp;

                if (SwitchSwizzler.Pow2RoundUp(SwitchSwizzler.DivRoundUp(mipmapHeight, blkWidth)) < linesPerBlockHeight)
                {
                    blockHeightShift += 1;
                }

                uint roundWidth = SwitchSwizzler.DivRoundUp(mipmapWidth, blkWidth);
                uint roundHeight = SwitchSwizzler.DivRoundUp(mipmapHeight, blkHeight);

                surfaceSize += SwitchSwizzler.RoundUp(surfaceSize, dataAlignment) - surfaceSize;
                mipOffsets.Add(surfaceSize);

                var msize = (int)(mipOffsets[0] + image.Length - mipOffsets[i]);

                var mipmap = new byte[msize];
                Array.Copy(image, mipOffsets[i], mipmap, 0, msize);

                var info = new SwizzleInfo
                {
                    Width = mipmapWidth,
                    Height = mipmapHeight,
                    Depth = 1,
                    BlkWidth = blkWidth,
                    BlkHeight = blkHeight,
                    BlkDepth = 1,
                    RoundPitch = 1,
                    Bpp = bpp,
                    TileMode = 0,
                    BlockHeightLog2 = (int)Math.Max(0, blockHeightLog2 - blockHeightShift),
                };

                uint pitch = SwitchSwizzler.RoundUp(roundWidth * bpp, 64);
                surfaceSize += pitch *
                               SwitchSwizzler.RoundUp(roundHeight, Math.Max(1, blockHeight >> blockHeightShift) * 8);
                byte[] deswizzled = SwitchSwizzler.Deswizzle(info, mipmap);
                Array.Copy(deswizzled, 0, result, resultOffset, mipmapSize);
                resultOffset += mipmapSize;
            }

            return result;
        }
    }
}