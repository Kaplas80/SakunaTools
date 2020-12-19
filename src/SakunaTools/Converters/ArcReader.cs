// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Converters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using SakunaTools.Types;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Uncompressed ARC container reader.
    /// </summary>
    public class ArcReader : IConverter<BinaryFormat, NodeContainerFormat>
    {
        /// <summary>
        /// Converts an ARC container into a NodeContainerFormat.
        /// </summary>
        /// <param name="source">The source format.</param>
        /// <returns>The node container.</returns>
        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "Ownserhip dispose transferred")]
        public NodeContainerFormat Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.Stream.Position = 0;

            var result = new NodeContainerFormat();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var reader = new DataReader(source.Stream)
            {
                DefaultEncoding = Encoding.GetEncoding(1252),
                Endianness = EndiannessMode.LittleEndian,
            };

            // Read the file header
            ArcHeader header = reader.Read<ArcHeader>() as ArcHeader;
            this.CheckHeader(header);

            if (header.CompressionType == 0x02)
            {
                // File is compressed
                throw new FormatException($"ARC: File is compressed.");
            }

            int startData = 0x10 + (header.FileCount * 0x070);
            for (var i = 0; i < header.FileCount; i++)
            {
                ArcFileInfo info = reader.Read<ArcFileInfo>() as ArcFileInfo;
                var binaryFormat = new BinaryFormat(source.Stream, startData + info.Offset, info.Size);

                string path = info.FileName.Trim('\0').Replace("\\", "/");
                int lastSeparator = path.LastIndexOf('/');
                string name = path.Substring(lastSeparator + 1);
                path = path.Substring(0, lastSeparator);

                var node = new Node(name, binaryFormat)
                {
                    Tags =
                    {
                        ["Unknown1"] = info.Unknown1,
                        ["Unknown2"] = info.Unknown2,
                    },
                };

                NodeFactory.CreateContainersForChild(result.Root, path, node);
            }

            return result;
        }

        private void CheckHeader(ArcHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (header.MagicId != "TGP0")
            {
                throw new FormatException($"ARC: Bad magic Id ({header.MagicId} != TGP0)");
            }
        }
    }
}