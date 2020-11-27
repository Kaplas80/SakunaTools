// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------

namespace SakunaTools.Formats.Sections
{
    using SakunaTools.Enums;

    /// <summary>
    /// This class holds information common to all font characters.
    /// </summary>
    public class CommonSection : Section
    {
        /// <summary>
        /// Gets or sets the distance in pixels between each line of text.
        /// </summary>
        public ushort LineHeight { get; set; }

        /// <summary>
        /// Gets or sets the number of pixels from the absolute top of the line to the base of the characters.
        /// </summary>
        public ushort Base { get; set; }

        /// <summary>
        /// Gets or sets the width of the texture, normally used to scale the x pos of the character image.
        /// </summary>
        public ushort ScaleWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the texture, normally used to scale the y pos of the character image.
        /// </summary>
        public ushort ScaleHeight { get; set; }

        /// <summary>
        /// Gets or sets the number of texture pages included in the font.
        /// </summary>
        public ushort PageCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the monochrome characters have been packed into each of the texture channels.
        /// <remarks>In this case <see cref="AlphaChannel"/> describes what is stored in each channel.</remarks>
        /// </summary>
        public bool IsPacked { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ChannelData"/> value for alpha channel.
        /// </summary>
        public ChannelData AlphaChannel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ChannelData"/> value for red channel.
        /// </summary>
        public ChannelData RedChannel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ChannelData"/> value for green channel.
        /// </summary>
        public ChannelData GreenChannel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ChannelData"/> value for blue channel.
        /// </summary>
        public ChannelData BlueChannel { get; set; }

        /// <inheritdoc />
        public override int SectionSize => 15;
    }
}