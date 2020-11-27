// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Formats
{
    using System;
    using System.Collections.Generic;
    using SakunaTools.Formats.Sections;
    using Yarhl.FileFormat;

    /// <summary>
    /// AngelCode BMFont format.
    /// <see href="https://www.angelcode.com/products/BMFont/doc/file_format.html"/>.
    /// </summary>
    public class BMFont : IFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BMFont"/> class.
        /// </summary>
        public BMFont()
        {
            this.Pages = new List<string>();
            this.Chars = new Dictionary<uint, CharacterSection>();
            this.Kernings = new Dictionary<Tuple<uint, uint>, short>();
        }

        /// <summary>
        /// Gets or sets the font generation info.
        /// </summary>
        public InfoSection Info { get; set; }

        /// <summary>
        /// Gets or sets the font common info.
        /// </summary>
        public CommonSection Common { get; set; }

        /// <summary>
        /// Gets the page (texture) names.
        /// </summary>
        public IList<string> Pages { get; }

        /// <summary>
        /// Gets the dictionary with the character info.
        /// </summary>
        public IDictionary<uint, CharacterSection> Chars { get; }

        /// <summary>
        /// Gets the dictionary with the kerning pair info.
        /// </summary>
        public IDictionary<Tuple<uint, uint>, short> Kernings { get; }
    }
}