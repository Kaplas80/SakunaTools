// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using SakunaTools.Formats;
    using SakunaTools.Formats.Sections;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    /// <summary>
    /// BMFont config file writer.
    /// </summary>
    public class ConfigWriter : IConverter<BMFont, BinaryFormat>
    {
        /// <summary>
        /// Converts a font into a BMFont config file.
        /// </summary>
        /// <param name="source">BMFont object.</param>
        /// <returns>The BinaryFormat.</returns>
        /// <exception cref="ArgumentNullException">The source is null.</exception>
        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "Ownserhip dispose transferred")]
        public BinaryFormat Convert(BMFont source)
        {
             if (source == null)
             {
                 throw new ArgumentNullException(nameof(source));
             }

             DataStream dataStream = DataStreamFactory.FromMemory();

             var writer = new TextWriter(dataStream, Encoding.UTF8);

             writer.WriteLine("# AngelCode Bitmap Font Generator configuration file");
             writer.WriteLine("fileVersion=1");
             writer.WriteLine();

             WriteInfoBlock(source.Info, writer);
             WriteCommonBlock(source.Common, writer);
             WriteCharsBlock(source.Chars, writer);

             var result = new BinaryFormat(dataStream);
             return result;
        }

        private static void WriteInfoBlock(InfoSection info, TextWriter writer)
        {
            writer.WriteLine("# font settings");
            writer.WriteLine($"fontName={info.FontName}");
            writer.WriteLine("fontFile=");
            writer.WriteLine($"charSet={info.CharSet}");
            writer.WriteLine($"fontSize={info.Size}");
            writer.WriteLine($"aa={info.AntiAliasing}");
            writer.WriteLine($"scaleH={info.StretchHeight}");
            writer.WriteLine($"useSmoothing={(info.SmoothEnabled ? 1 : 0)}");
            writer.WriteLine($"isBold={(info.IsBold ? 1 : 0)}");
            writer.WriteLine($"isItalic={(info.IsItalic ? 1 : 0)}");
            writer.WriteLine($"useUnicode={(info.IsUnicode ? 1 : 0)}");
            writer.WriteLine("disableBoxChars=1");
            writer.WriteLine("outputInvalidCharGlyph=0");
            writer.WriteLine("dontIncludeKerningPairs=0");
            writer.WriteLine("useHinting=1");
            writer.WriteLine("renderFromOutline=0");
            writer.WriteLine("useClearType=1");
            writer.WriteLine("autoFitNumPages=0");
            writer.WriteLine("autoFitFontSizeMin=0");
            writer.WriteLine("autoFitFontSizeMax=0");
            writer.WriteLine();
            writer.WriteLine("# character alignment");
            writer.WriteLine($"paddingDown={info.Padding.Bottom}");
            writer.WriteLine($"paddingUp={info.Padding.Top}");
            writer.WriteLine($"paddingRight={info.Padding.Right}");
            writer.WriteLine($"paddingLeft={info.Padding.Left}");
            writer.WriteLine($"spacingHoriz={info.Spacing.Horizontal}");
            writer.WriteLine($"spacingVert={info.Spacing.Vertical}");
            writer.WriteLine($"useFixedHeight={(info.IsFixedHeight ? 1 : 0)}");
            writer.WriteLine("forceZero=0");
            writer.WriteLine("widthPaddingFactor=0.00");
            writer.WriteLine();
            writer.WriteLine("# outline");
            writer.WriteLine($"outlineThickness={info.Outline}");
            writer.WriteLine();
        }

        private static void WriteCommonBlock(CommonSection common, TextWriter writer)
        {
            writer.WriteLine("# output file");
            writer.WriteLine($"outWidth={common.ScaleWidth}");
            writer.WriteLine($"outHeight={common.ScaleHeight}");
            writer.WriteLine("outBitDepth=32");
            writer.WriteLine("fontDescFormat=2");
            writer.WriteLine($"fourChnlPacked={(common.IsPacked ? 1 : 0)}");
            writer.WriteLine("textureFormat=tga");
            writer.WriteLine("textureCompression=0");
            writer.WriteLine($"alphaChnl={(byte)common.AlphaChannel}");
            writer.WriteLine($"redChnl={(byte)common.RedChannel}");
            writer.WriteLine($"greenChnl={(byte)common.GreenChannel}");
            writer.WriteLine($"blueChnl={(byte)common.BlueChannel}");
            writer.WriteLine("invA=0");
            writer.WriteLine("invR=0");
            writer.WriteLine("invG=0");
            writer.WriteLine("invB=0");
            writer.WriteLine();
        }

        private static void WriteCharsBlock(IDictionary<uint, CharacterSection> chars, TextWriter writer)
        {
            var selectedChars = chars.Keys.ToList();
            selectedChars.Sort();

            writer.WriteLine("# selected chars");
            uint? lastChar = null;
            var isRange = false;
            var lineLength = 0;
            for (var i = 0; i < selectedChars.Count; i++)
            {
                if (lastChar == null)
                {
                    var text = $"chars={selectedChars[i]}";
                    writer.Write(text);
                    lineLength += text.Length;
                }
                else if (lastChar != selectedChars[i] - 1)
                {
                    var text = $",{selectedChars[i]}";
                    writer.Write(text);
                    lineLength += text.Length;
                }

                if (lastChar == selectedChars[i] - 1)
                {
                    if (!isRange)
                    {
                        writer.Write("-");
                        lineLength += 1;
                        isRange = true;
                    }

                    if ((i == selectedChars.Count - 1) || (selectedChars[i] + 1 != selectedChars[i + 1]))
                    {
                        var text = $"{selectedChars[i]}";
                        writer.Write(text);
                        lineLength += text.Length;
                        isRange = false;
                    }
                }

                lastChar = selectedChars[i];

                if (lineLength <= 100 || isRange)
                {
                    continue;
                }

                writer.WriteLine();
                lineLength = 0;
                lastChar = null;
            }

            writer.WriteLine();
        }
    }
}