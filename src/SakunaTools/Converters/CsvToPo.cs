// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools.Converters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using SakunaTools.Types;
    using Yarhl.FileFormat;
    using Yarhl.IO;
    using Yarhl.Media.Text;

    /// <summary>
    /// Converts a CSV file into a PO file.
    /// </summary>
    public class CsvToPo : IConverter<BinaryFormat, Po>, IInitializer<CsvParameters>
    {
        private CsvParameters parameters = new CsvParameters
        {
            FileExtension = ".csvtxt",
        };

        /// <summary>
        /// Initialize parameters.
        /// </summary>
        /// <param name="parameters">Parameters.</param>
        public void Initialize(CsvParameters parameters)
        {
            this.parameters = parameters;
        }

        /// <summary>
        /// Converts a binary format into a Po.
        /// </summary>
        /// <param name="source">The source format.</param>
        /// <returns>A PO format.</returns>
        [SuppressMessage(
            "Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "Ownserhip dispose transferred")]
        public Po Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.Stream.Position = 0;
            var reader = new Yarhl.IO.TextReader(source.Stream, Encoding.UTF8);
            string data = reader.ReadToEnd();
            data = data.Replace("\n", "<NewLine>", StringComparison.InvariantCulture);

            string[] lines = data.Split('\r');

            string csvHeader = lines[0];

            var poHeader = new PoHeader("Sakuna: Of Rice and Ruin", "dummy@dummy.com", "es");
            var po = new Po(poHeader);
            po.Header.Extensions.Add("CSVHeader", csvHeader);
            po.Header.Extensions.Add("FileExtension", this.parameters.FileExtension);

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

            if (translationIndexes.All(x => x == -1))
            {
                throw new FormatException("File without English fields.");
            }

            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i]))
                {
                    continue;
                }

                string[] split = lines[i].Split(',');

                for (var j = 0; j < translationIndexes.Length; j++)
                {
                    if (translationIndexes[j] == -1)
                    {
                        continue;
                    }

                    var entry = new PoEntry();

                    string original = split[translationIndexes[j]];

                    if (string.IsNullOrEmpty(original))
                    {
                        original = "<!empty>";
                    }

                    entry.Context = $"{translationFieldArray[j]}\n{lines[i]}";
                    entry.Original = original.Replace("\\1", ",").Replace("<NewLine>", "\n");
                    po.Add(entry);
                }
            }

            return po;
        }
    }
}