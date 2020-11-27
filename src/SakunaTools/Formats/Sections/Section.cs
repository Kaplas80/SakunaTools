// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------

namespace SakunaTools.Formats.Sections
{
    /// <summary>
    /// Section of the BMFont format.
    /// </summary>
    public abstract class Section
    {
        /// <summary>
        /// Gets the section size (in bytes).
        /// </summary>
        public abstract int SectionSize { get; }
    }
}