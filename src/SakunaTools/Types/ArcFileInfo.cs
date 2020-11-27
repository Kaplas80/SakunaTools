// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Types
{
    using Yarhl.IO.Serialization.Attributes;

    /// <summary>
    /// File information stored in ARC container.
    /// </summary>
    [Serializable]
    public class ArcFileInfo
    {
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        [BinaryString(FixedSize = 0x60, Terminator = null)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file offset inside the container.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the real size (not compressed).
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the unknown1 value.
        /// </summary>
        public int Unknown1 { get; set; }

        /// <summary>
        /// Gets or sets the unknown2 value.
        /// </summary>
        public int Unknown2 { get; set; }
    }
}