// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Types
{
    using Yarhl.IO.Serialization.Attributes;

    /// <summary>
    /// Header of the ARC container.
    /// </summary>
    [Serializable]
    public class ArcHeader
    {
        /// <summary>
        /// Gets or sets the file id.
        /// </summary>
        [BinaryString(FixedSize = 4, Terminator = null)]
        public string MagicId { get; set; }

        /// <summary>
        /// Gets or sets the file version.
        /// </summary>
        public short Version { get; set; }

        /// <summary>
        /// Gets or sets the compression type.
        /// 0 = not compressed.
        /// 2 = LZ4 compression.
        /// </summary>
        public short CompressionType { get; set; }

        /// <summary>
        /// Gets or sets the number of files inside the container.
        /// </summary>
        public int FileCount { get; set; }

        /// <summary>
        /// Gets or sets the uncompressed data size.
        /// </summary>
        public int OriginalSize { get; set; }
    }
}