// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------

namespace SakunaTools.Enums
{
    using System;

    /// <summary>
    /// The texture channels where the character image is found.
    /// </summary>
    [Flags]
    public enum TextureChannels
    {
        /// <summary>
        /// Blue channel.
        /// </summary>
        Blue = 1,

        /// <summary>
        /// Green channel.
        /// </summary>
        Green = 2,

        /// <summary>
        /// Red channel.
        /// </summary>
        Red = 4,

        /// <summary>
        /// Alpha channel.
        /// </summary>
        Alpha = 8,
    }
}