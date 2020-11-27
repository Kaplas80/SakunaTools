// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------

namespace SakunaTools.Formats.Sections
{
    using SakunaTools.Enums;

    /// <summary>
    /// This class represents how the font was generated.
    /// </summary>
    public class InfoSection : Section
    {
        /// <summary>
        /// Gets or sets the name of the true type font.
        /// </summary>
        public string FontName { get; set; }

        /// <summary>
        /// Gets or sets the size of the true type font.
        /// </summary>
        public short Size { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the font is bold.
        /// </summary>
        public bool IsBold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the font is italic.
        /// </summary>
        public bool IsItalic { get; set; }

        /// <summary>
        /// Gets or sets the OEM charset used (when not unicode).
        /// </summary>
        public byte CharSet { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the font is using the Unicode charset.
        /// </summary>
        public bool IsUnicode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the font is fixed height.
        /// </summary>
        public bool IsFixedHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether smoothing is turned on.
        /// </summary>
        public bool SmoothEnabled { get; set; }

        /// <summary>
        /// Gets or sets the font height stretch in percentage.
        /// <remarks>100% means no stretch</remarks>
        /// </summary>
        public ushort StretchHeight { get; set; }

        /// <summary>
        /// Gets or sets the super sampling level used.
        /// <remarks>1 means no super sampling was used.</remarks>
        /// </summary>
        public byte AntiAliasing { get; set; }

        /// <summary>
        /// Gets or sets the padding for each character.
        /// </summary>
        public CharacterPadding Padding { get; set; }

        /// <summary>
        /// Gets or sets the spacing for each character.
        /// </summary>
        public CharacterSpacing Spacing { get; set; }

        /// <summary>
        /// Gets or sets the outline thickness for the characters.
        /// </summary>
        public byte Outline { get; set; }

        /// <inheritdoc />
        public override int SectionSize => 15 + this.FontName.Length;
    }
}