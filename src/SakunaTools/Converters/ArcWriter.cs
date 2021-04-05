// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using SakunaTools.Types;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Uncompressed ARC container writer.
    /// </summary>
    public class ArcWriter : IConverter<NodeContainerFormat, BinaryFormat>
    {
        /// <summary>
        /// Converts a NodeContainerFormat into an ARC container.
        /// </summary>
        /// <param name="source">The source format.</param>
        /// <returns>The binary format.</returns>
        public BinaryFormat Convert(NodeContainerFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var files = new List<Node>();
            var tags = new Dictionary<string, Dictionary<string, int>>();
            var totalSize = 0;

            foreach (Node node in Navigator.IterateNodes(source.Root, NavigationMode.DepthFirst))
            {
                if (node.IsContainer)
                {
                    continue;
                }

                if (node.Name.EndsWith(".arcinfo", StringComparison.InvariantCulture))
                {
                    string path = node.Path.Replace(".arcinfo", string.Empty);
                    var dict = new Dictionary<string, int>();

                    var reader = new TextDataReader(node.Stream);
                    while (!node.Stream.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        string[] split = line.Split('=');
                        dict.Add(split[0], int.Parse(split[1], NumberStyles.Integer, CultureInfo.InvariantCulture));
                    }

                    tags.Add(path, dict);
                }
                else
                {
                    files.Add(node);
                    totalSize += (int)node.Stream.Length;
                }
            }

            var header = new ArcHeader
            {
                MagicId = "TGP0",
                Version = 3,
                CompressionType = 0,
                FileCount = files.Count,
                OriginalSize = totalSize,
            };

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            DataStream outputStream = DataStreamFactory.FromMemory();
            var writer = new DataWriter(outputStream)
            {
                DefaultEncoding = Encoding.GetEncoding(1252),
                Endianness = EndiannessMode.LittleEndian,
            };

            writer.WriteOfType(header);
            var currentOffset = 0;
            foreach (Node node in files)
            {
                string path = node.Path.Substring(source.Root.Path.Length + 1).Replace("/", "\\");
                var size = (int)node.Stream.Length;
                writer.Write(path, 0x60, false);
                writer.Write(currentOffset);
                writer.Write(size);

                if (tags.ContainsKey(node.Path))
                {
                    writer.Write(tags[node.Path]["Unknown1"]);
                    writer.Write(tags[node.Path]["Unknown2"]);
                }
                else
                {
                    writer.Write(0);
                    writer.Write(0);
                }

                currentOffset += size;
            }

            foreach (Node node in files)
            {
                node.Stream.WriteTo(outputStream);
            }

            return new BinaryFormat(outputStream);
        }
    }
}