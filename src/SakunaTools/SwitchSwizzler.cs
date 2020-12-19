// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools
{
    using System;
    using SakunaTools.Types;

    /// <summary>
    /// Code from: https://github.com/KillzXGaming/Switch-Toolbox/blob/master/Switch_Toolbox_Library/Texture%20Decoding/Switch/TegraX1Swizzle.cs.
    /// </summary>
    public static class SwitchSwizzler
    {
        /// <summary>
        /// Remove swizzling from image.
        /// </summary>
        /// <param name="info">Image and swizzle information.</param>
        /// <param name="data">Swizzled image data.</param>
        /// <returns>The deswizzled image data.</returns>
        public static byte[] Deswizzle(SwizzleInfo info, byte[] data)
        {
            return info != null ? Swizzle(info, data, 0) : data;
        }

        /// <summary>
        /// Swizzles an image.
        /// </summary>
        /// <param name="info">Image and swizzle information.</param>
        /// <param name="data">Image data.</param>
        /// <returns>The swizzled image data.</returns>
        public static byte[] Swizzle(SwizzleInfo info, byte[] data)
        {
            return info != null ? Swizzle(info, data, 1) : data;
        }

        /// <summary>
        /// Gets the block height.
        /// </summary>
        /// <param name="height">Image height.</param>
        /// <returns>The block height.</returns>
        public static uint GetBlockHeight(uint height)
        {
            uint blockHeight = Pow2RoundUp(height / 8);
            if (blockHeight > 16)
            {
                blockHeight = 16;
            }

            return blockHeight;
        }

        /// <summary>
        /// Makes the division of 2 unsigned integers, rounding up the result.
        /// </summary>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        /// <returns>The result of dividend/divisor.</returns>
        public static uint DivRoundUp(uint dividend, uint divisor)
        {
            return (dividend + divisor - 1) / divisor;
        }

        /// <summary>
        /// Rounds up a unsigned integer to a multiple of other number.
        /// </summary>
        /// <param name="x">The integer to round up.</param>
        /// <param name="y">The other number.</param>
        /// <returns>The rounded up value.</returns>
        public static uint RoundUp(uint x, uint y)
        {
            return ((x - 1) | (y - 1)) + 1;
        }

        /// <summary>
        /// Rounds up a unsigned integer to a power of 2.
        /// </summary>
        /// <param name="x">The integer to round up.</param>
        /// <returns>The rounded up value.</returns>
        public static uint Pow2RoundUp(uint x)
        {
            x -= 1;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }

        private static byte[] Swizzle(SwizzleInfo info, byte[] data, int toSwizzle)
        {
            var blockHeight = (uint)(1 << info.BlockHeightLog2);

            uint width = DivRoundUp(info.Width, info.BlkWidth);
            uint height = DivRoundUp(info.Height, info.BlkHeight);

            uint pitch;
            uint surfaceSize;
            if (info.TileMode == 1)
            {
                pitch = width * info.Bpp;

                if (info.RoundPitch == 1)
                {
                    pitch = RoundUp(pitch, 32);
                }

                surfaceSize = pitch * height;
            }
            else
            {
                pitch = RoundUp(width * info.Bpp, 64);
                surfaceSize = pitch * RoundUp(height, blockHeight * 8);
            }

            var result = new byte[surfaceSize];

            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    uint swizzledPosition;

                    if (info.TileMode == 1)
                    {
                        swizzledPosition = (y * pitch) + (x * info.Bpp);
                    }
                    else
                    {
                        swizzledPosition = GetAddrBlockLinear(x, y, width, info.Bpp, 0, blockHeight);
                    }

                    uint originalPosition = ((y * width) + x) * info.Bpp;

                    if (swizzledPosition + info.Bpp > surfaceSize)
                    {
                        continue;
                    }

                    if (toSwizzle == 0)
                    {
                        Array.Copy(data, swizzledPosition, result, originalPosition, info.Bpp);
                    }
                    else
                    {
                        Array.Copy(data, originalPosition, result, swizzledPosition, info.Bpp);
                    }
                }
            }

            return result;
        }

        private static uint GetAddrBlockLinear(uint x, uint y, uint width, uint bpp, uint baseAddress, uint blockHeight)
        {
            // From Tegra X1 TRM
            uint imageWidthInGobs = DivRoundUp(width * bpp, 64);
            uint gobAddress = baseAddress +
                              ((y / (8 * blockHeight)) * 512 * blockHeight * imageWidthInGobs) +
                              ((x * bpp / 64) * 512 * blockHeight) +
                              (((y % (8 * blockHeight)) / 8) * 512);
            x *= bpp;
            uint address = gobAddress +
                           (((x % 64) / 32) * 256) +
                           (((y % 8) / 2) * 64) +
                           (((x % 32) / 16) * 32) +
                           ((y % 2) * 16) +
                           (x % 16);
            return address;
        }
    }
}