// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Types
{
    /// <summary>
    /// Parameters for csv conversion.
    /// </summary>
    public class CsvParameters
    {
        /// <summary>
        /// Gets or sets the file extension.
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Gets or sets the translatable fields (separated by comma).
        /// </summary>
        public string TranslationFields { get; set; }
    }
}