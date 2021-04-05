// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using SakunaTools.Types;
    using Yarhl.IO;

    /// <summary>
    /// Converts a TGA into a nhtex swizzled for Nintendo Switch.
    /// </summary>
    public class TgaToSwitchNhtex : TgaToNhtex
    {
        /// <summary>
        /// Write the swizzled mipmaps to a DataStream.
        /// </summary>
        /// <param name="height">Image height (in pixels).</param>
        /// <param name="width">Image width (in pixels).</param>
        /// <param name="mipmaps">The list of mipmaps.</param>
        /// <returns>The DataStream.</returns>
        protected override DataStream Write(uint height, uint width, List<byte[]> mipmaps)
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

            writer.Write(0x20L);
            writer.Write(0x18L);
            writer.Write(0x28L);
            writer.Write(0x00L); // Size - 0x38

            writer.Write(width);
            writer.Write(height);
            writer.Write(0x01);
            writer.Write(0x01);

            writer.Write(0x49);
            writer.Write(mipmaps.Count);

            var blockHeightShift = 0;

            const uint blkWidth = 4;
            const uint blkHeight = 4;
            const uint bpp = 8;

            uint blockHeight = SwitchSwizzler.GetBlockHeight(SwitchSwizzler.DivRoundUp(height, blkHeight));
            uint blockHeightLog2 = (uint)System.Convert.ToString(blockHeight, 2).Length - 1;

            uint linesPerBlockHeight = blockHeight * 8;

            for (var mipLevel = 0; mipLevel < mipmaps.Count; mipLevel++)
            {
                byte[] mipmap = mipmaps[mipLevel];

                uint mipmapWidth = Math.Max(1, width >> mipLevel);
                uint mipmapHeight = Math.Max(1, height >> mipLevel);

                uint roundedHeight = SwitchSwizzler.DivRoundUp(mipmapHeight, blkHeight);

                if (SwitchSwizzler.Pow2RoundUp(roundedHeight) < linesPerBlockHeight)
                {
                    blockHeightShift += 1;
                }

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

                byte[] swizzled = SwitchSwizzler.Swizzle(info, mipmap);
                writer.Write(swizzled);
            }

            writer.Stream.Seek(0x18, System.IO.SeekOrigin.Begin);
            writer.Write(outputDataStream.Length - 0x30);

            writer.Stream.Seek(0x48, System.IO.SeekOrigin.Begin);
            writer.Write(outputDataStream.Length - 0x68);

            return outputDataStream;
        }
    }
}