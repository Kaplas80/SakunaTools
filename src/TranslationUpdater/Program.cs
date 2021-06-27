// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace TranslationUpdater
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using CommandLine;
    using Yarhl.FileSystem;
    using Yarhl.Media.Text;

    /// <summary>
    /// Main program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Arguments.</param>
        public static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(Run);
        }

        private static void Run(Options opts)
        {
            if (!Directory.Exists(opts.Input))
            {
                Console.WriteLine("Directory not found!!!");
                return;
            }

            string oldPath = Path.Combine(opts.Input, "old");
            string newPath = Path.Combine(opts.Input, "new");

            string[] newPoFiles = Directory.GetFiles(newPath, "*.po", SearchOption.AllDirectories);
            foreach (string newPoFile in newPoFiles)
            {
                Console.WriteLine($"Updating {newPoFile}...");
                string oldPoFile = newPoFile.Replace(newPath, oldPath);

                if (!File.Exists(oldPoFile))
                {
                    Console.WriteLine($"ERROR! {oldPoFile} not found!!!");
                    continue;
                }

                UpdateTranslation(oldPoFile, newPoFile);
            }

            Console.Write("Finished! Press RETURN to close...");
            Console.ReadLine();
        }

        private static void UpdateTranslation(string oldFile, string newFile)
        {
            Node oldFileNode = NodeFactory.FromFile(oldFile);
            Node newFileNode = NodeFactory.FromFile(newFile);

            oldFileNode.TransformWith<Binary2Po>();
            newFileNode.TransformWith<Binary2Po>();

            Po oldPo = oldFileNode.GetFormatAs<Po>();
            Po newPo = newFileNode.GetFormatAs<Po>();

            foreach (PoEntry entry in newPo.Entries)
            {
                string[] context = entry.Context.Split('\n');
                string field = context[0];
                string id = context[1].Split(',')[0];

                // Find "field + id" in old Po
                PoEntry oldEntry = oldPo.Entries.FirstOrDefault(x => x.Context.StartsWith($"{field}\n{id}"));
                if (oldEntry == null)
                {
                    Console.WriteLine($"Translation for \"{field} {id}\" not found.");
                    continue;
                }

                entry.Translated = oldEntry.Translated;
            }

            newFileNode.TransformWith<Po2Binary>();
            newFileNode.Stream.WriteTo(newFile);
        }

        [SuppressMessage(
            "ReSharper",
            "CA1812:ClassNeverInstantiated.Local",
            Justification = "Instantiated by reflection.")]
        private class Options
        {
            [Value(0, Required = true, HelpText = "Input directory (must have \"old\" and \"new\" subdirectories).")]
            public string Input { get; set; }
        }
    }
}
