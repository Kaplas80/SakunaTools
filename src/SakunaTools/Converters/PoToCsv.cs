// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Yarhl.FileFormat;
    using Yarhl.IO;
    using Yarhl.Media.Text;

    /// <summary>
    /// Converts a PO file into a CSV.
    /// </summary>
    public class PoToCsv : IConverter<Po, BinaryFormat>
    {
        /// <summary>
        /// Converts a Po into a CSV.
        /// </summary>
        /// <param name="source">The source format.</param>
        /// <returns>The csv.</returns>
        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "Ownserhip dispose transferred")]
        public BinaryFormat Convert(Po source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            DataStream outputDataStream = DataStreamFactory.FromMemory();

            var writer = new Yarhl.IO.TextDataWriter(outputDataStream, Encoding.UTF8)
            {
                NewLine = "\r",
            };

            string csvHeader = source.Header.Extensions["CSVHeader"];
            writer.WriteLine(csvHeader);

            string[] fields = csvHeader.Split(',');
            string[] translationFieldArray = SakunaTools.Constants.TranslationFields.Split(',');
            var translationIndexes = new int[translationFieldArray.Length];
            for (var i = 0; i < translationIndexes.Length; i++)
            {
                translationIndexes[i] = -1;
            }

            for (var i = 0; i < fields.Length; i++)
            {
                int index = Array.IndexOf(translationFieldArray, fields[i]);

                if (index != -1)
                {
                    translationIndexes[index] = i;
                }
            }

            var translations = new Dictionary<string, Dictionary<int, string>>();

            foreach (PoEntry entry in source.Entries)
            {
                string[] split = entry.Context.Split('\n');
                string context;
                string field;
                if (split.Length == 1)
                {
                    context = split[0];
                    field = "English";
                }
                else
                {
                    context = split[1];
                    field = split[0];
                }

                if (!translations.ContainsKey(context))
                {
                    translations.Add(context, new Dictionary<int, string>());
                }

                string value;
                if (entry.Original == "<!empty>")
                {
                    value = string.Empty;
                }
                else if (string.IsNullOrEmpty(entry.Translated))
                {
                    value = entry.Original;
                }
                else
                {
                    value = entry.Translated;
                }

                int index = Array.IndexOf(translationFieldArray, field);

                if (index >= 0 && translationIndexes[index] >= 0)
                {
                    translations[context].Add(translationIndexes[index], value);
                }
            }

            foreach ((string context, Dictionary<int, string> translation) in translations)
            {
                string[] split = context.Split(',');
                foreach ((int field, string value) in translation)
                {
                    split[field] = value.Replace(",", "\\1").Replace("\n", "<NewLine>");
                }

                var line = string.Join(',', split);
                writer.WriteLine(line.Replace("<NewLine>", "\n"));
            }

            return new BinaryFormat(outputDataStream);
        }
    }
}