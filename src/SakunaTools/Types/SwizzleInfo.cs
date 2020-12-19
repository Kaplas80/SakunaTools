// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Types
{
    /// <summary>
    /// Image and swizzling information.
    /// </summary>
    public class SwizzleInfo
    {
        /// <summary>
        /// Gets or sets the image width.
        /// </summary>
        public uint Width { get; set; }

        /// <summary>
        /// Gets or sets the image height.
        /// </summary>
        public uint Height { get; set; }

        /// <summary>
        /// Gets or sets the image depth.
        /// </summary>
        public uint Depth { get; set; }

        /// <summary>
        /// Gets or sets the swizzling blk width.
        /// </summary>
        public uint BlkWidth { get; set; }

        /// <summary>
        /// Gets or sets the swizzling blk height.
        /// </summary>
        public uint BlkHeight { get; set; }

        /// <summary>
        /// Gets or sets the swizzling blk depth.
        /// </summary>
        public uint BlkDepth { get; set; }

        /// <summary>
        /// Gets or sets the image round pitch.
        /// </summary>
        public int RoundPitch { get; set; }

        /// <summary>
        /// Gets or sets the image bpp.
        /// </summary>
        public uint Bpp { get; set; }

        /// <summary>
        /// Gets or sets the image tile mode.
        /// </summary>
        public uint TileMode { get; set; }

        /// <summary>
        /// Gets or sets the image block height (log2).
        /// </summary>
        public int BlockHeightLog2 { get; set; }
    }
}