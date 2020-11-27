// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------

namespace SakunaTools.Formats.Sections
{
    using SakunaTools.Enums;

    /// <summary>
    /// This class describes characters in the font.
    /// </summary>
    public class CharacterSection : Section
    {
        /// <summary>
        /// Gets or sets the left position of the character image in the texture.
        /// </summary>
        public ushort X { get; set; }

        /// <summary>
        /// Gets or sets the top position of the character image in the texture.
        /// </summary>
        public ushort Y { get; set; }

        /// <summary>
        /// Gets or sets the width of the character image in the texture.
        /// </summary>
        public ushort Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the character image in the texture.
        /// </summary>
        public ushort Height { get; set; }

        /// <summary>
        /// Gets or sets how much the current position should be offset when
        /// copying the image from the texture to the screen.
        /// </summary>
        public short XOffset { get; set; }

        /// <summary>
        /// Gets or sets how much the current position should be offset when
        /// copying the image from the texture to the screen.
        /// </summary>
        public short YOffset { get; set; }

        /// <summary>
        /// Gets or sets how much the current position should be advanced after drawing the character.
        /// </summary>
        public short XAdvance { get; set; }

        /// <summary>
        /// Gets or sets the texture page where the character image is found.
        /// </summary>
        public byte Page { get; set; }

        /// <summary>
        /// Gets or sets the texture channel where the character image is found.
        /// </summary>
        public TextureChannels Channel { get; set; }

        /// <inheritdoc />
        public override int SectionSize => 20;
    }
}