// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Converters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using SakunaTools.Enums;
    using SakunaTools.Formats;
    using SakunaTools.Formats.Sections;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    /// <summary>
    /// BMFont file reader (binary format).
    /// </summary>
    public class FntReader : IConverter<BinaryFormat, BMFont>
    {
        /// <summary>
        /// Reads a BMFont binary file.
        /// </summary>
        /// <param name="source">Input data.</param>
        /// <returns>The BMFont object.</returns>
        /// <exception cref="ArgumentNullException">Source is null.</exception>
        /// <exception cref="FormatException">File is in incorrect format.</exception>
        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "Ownserhip dispose transferred")]
        public BMFont Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.Stream.Position = 0;

            var result = new BMFont();

            var reader = new DataReader(source.Stream)
            {
                DefaultEncoding = Encoding.UTF8,
                Endianness = EndiannessMode.LittleEndian,
            };

            string magicId = reader.ReadString(3);

            if (magicId != "BMF")
            {
                throw new FormatException("Binary BMFont: Bad magic Id.");
            }

            byte formatVersion = reader.ReadByte();
            if (formatVersion != 3)
            {
                throw new FormatException("Binary BMFont: Bad format version.");
            }

            while (!source.Stream.EndOfStream)
            {
                byte blockType = reader.ReadByte();
                int blockSize = reader.ReadInt32();

                switch (blockType)
                {
                    case 1: // info
                    {
                        if (result.Info != null)
                        {
                            throw new FormatException("Binary BMFont: Duplicated info block.");
                        }

                        result.Info = ReadInfoBlock(reader);
                        break;
                    }

                    case 2: // common
                    {
                        if (result.Common != null)
                        {
                            throw new FormatException("Binary BMFont: Duplicated common block.");
                        }

                        result.Common = ReadCommonBlock(reader);
                        break;
                    }

                    case 3: // pages
                    {
                        if (result.Common == null)
                        {
                            throw new FormatException("Binary BMFont: No common block before pages block.");
                        }

                        for (var i = 0; i < result.Common.PageCount; i++)
                        {
                            string pageName = reader.ReadString();
                            result.Pages.Add(pageName);
                        }

                        break;
                    }

                    case 4: // chars
                    {
                        int charCount = blockSize / 0x14;

                        for (var i = 0; i < charCount; i++)
                        {
                            uint characterId = reader.ReadUInt32();

                            if (result.Chars.ContainsKey(characterId))
                            {
                                throw new FormatException($"Binary BMFont: Duplicated char {characterId:X4}.");
                            }

                            CharacterSection character = ReadCharacter(reader);

                            result.Chars.Add(characterId, character);
                        }

                        break;
                    }

                    case 5: // kerning
                    {
                        int kerningCount = blockSize / 0x0A;

                        for (var i = 0; i < kerningCount; i++)
                        {
                            uint firstChar = reader.ReadUInt32();
                            uint secondChar = reader.ReadUInt32();
                            short amount = reader.ReadInt16();

                            var key = new Tuple<uint, uint>(firstChar, secondChar);
                            result.Kernings.Add(key, amount);
                        }

                        break;
                    }

                    default: // unknown block
                    {
                        throw new FormatException("Binary BMFont: Unknown block.");
                    }
                }
            }

            return result;
        }

        private static InfoSection ReadInfoBlock(DataReader reader)
        {
            var result = new InfoSection
            {
                Size = reader.ReadInt16(),
            };

            byte bitField = reader.ReadByte();
            result.SmoothEnabled = IsBitSet(bitField, 0);
            result.IsUnicode = IsBitSet(bitField, 1);
            result.IsItalic = IsBitSet(bitField, 2);
            result.IsBold = IsBitSet(bitField, 3);
            result.IsFixedHeight = IsBitSet(bitField, 4);

            result.CharSet = reader.ReadByte();
            result.StretchHeight = reader.ReadUInt16();
            result.AntiAliasing = reader.ReadByte();

            result.Padding = new CharacterPadding
            {
                Top = reader.ReadByte(),
                Right = reader.ReadByte(),
                Bottom = reader.ReadByte(),
                Left = reader.ReadByte(),
            };

            result.Spacing = new CharacterSpacing
            {
                Horizontal = reader.ReadByte(),
                Vertical = reader.ReadByte(),
            };

            result.Outline = reader.ReadByte();
            result.FontName = reader.ReadString();

            return result;
        }

        private static CommonSection ReadCommonBlock(DataReader reader)
        {
            var result = new CommonSection
            {
                LineHeight = reader.ReadUInt16(),
                Base = reader.ReadUInt16(),
                ScaleWidth = reader.ReadUInt16(),
                ScaleHeight = reader.ReadUInt16(),
                PageCount = reader.ReadUInt16(),
            };

            byte bitField = reader.ReadByte();
            result.IsPacked = IsBitSet(bitField, 7);

            result.AlphaChannel = (ChannelData)reader.ReadByte();
            result.RedChannel = (ChannelData)reader.ReadByte();
            result.GreenChannel = (ChannelData)reader.ReadByte();
            result.BlueChannel = (ChannelData)reader.ReadByte();

            return result;
        }

        private static CharacterSection ReadCharacter(DataReader reader)
        {
            return new CharacterSection
            {
                X = reader.ReadUInt16(),
                Y = reader.ReadUInt16(),
                Width = reader.ReadUInt16(),
                Height = reader.ReadUInt16(),
                XOffset = reader.ReadInt16(),
                YOffset = reader.ReadInt16(),
                XAdvance = reader.ReadInt16(),
                Page = reader.ReadByte(),
                Channel = (TextureChannels)reader.ReadByte(),
            };
        }

        private static bool IsBitSet(byte value, int bitNumber)
        {
            return (value & (1 << (7 - bitNumber))) != 0;
        }
    }
}