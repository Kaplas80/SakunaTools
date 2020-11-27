// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------

namespace SakunaTools.Enums
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Channel data enum.
    /// </summary>
    [SuppressMessage("ReSharper", "CA1717", Justification = "Name isn't plural.")]
    public enum ChannelData
    {
        /// <summary>
        /// The channel holds the glyph data
        /// </summary>
        Glyph = 0,

        /// <summary>
        /// The channel holds the outline
        /// </summary>
        Outline = 1,

        /// <summary>
        /// The channel holds the glyph and the outline
        /// </summary>
        GlyphAndOutline = 2,

        /// <summary>
        /// The channel is set to zero.
        /// </summary>
        Zero = 3,

        /// <summary>
        /// The channel is set to one.
        /// </summary>
        One = 4,
    }
}