// -----------------------------------------------------------------------
// Copyright (c) 2020 Kaplas. Licensed under MIT. See LICENSE for details.
// -----------------------------------------------------------------------

namespace SakunaTools
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using CommandLine;
    using SakunaTools.Converters;
    using SakunaTools.Types;
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
            if (Directory.Exists(opts.Input))
            {
                // It is a directory
                if (string.IsNullOrEmpty(opts.Output))
                {
                    opts.Output = string.Concat(opts.Input, ".arc");
                }

                BuildArc(opts.Input, opts.Output, opts.Overwrite);
            }
            else
            {
                // It is a file
                string extension = Path.GetExtension(opts.Input);
                switch (extension)
                {
                    case ".fnt": // BMFont binary file
                    {
                        if (string.IsNullOrEmpty(opts.Output))
                        {
                            opts.Output = opts.Input.Replace(".fnt", ".bmfc");
                        }

                        ConvertToBMFontConfig(opts.Input, opts.Output, opts.Overwrite);
                        break;
                    }

                    case ".nhtex": // Texture image
                    {
                        if (string.IsNullOrEmpty(opts.Output))
                        {
                            opts.Output = opts.Input.Replace(".nhtex", ".dds");
                        }

                        ConvertToDDS(opts.Input, opts.Output, opts.Overwrite);
                        break;
                    }

                    case ".tga": // Texture image
                    {
                        if (string.IsNullOrEmpty(opts.Output))
                        {
                            opts.Output = opts.Input.Replace(".tga", ".nhtex");
                        }

                        ConvertToNhtex(opts.Input, opts.Output, opts.Overwrite);
                        break;
                    }

                    case ".csvtxt": // Game text
                    case ".csvcr":
                    case ".csvq":
                    case ".csvwl":
                    {
                        if (string.IsNullOrEmpty(opts.Output))
                        {
                            opts.Output = opts.Input.Replace(extension, ".po");
                        }

                        ConvertToPo(opts.Input, opts.Output, opts.Overwrite, extension);
                        break;
                    }

                    case ".po": // Game text
                    {
                        ConvertToCsv(opts.Input, opts.Output, opts.Overwrite);
                        break;
                    }

                    case ".arc": // ARC archive
                    {
                        if (string.IsNullOrEmpty(opts.Output))
                        {
                            opts.Output = opts.Input.Replace(".arc", string.Empty);
                        }

                        ExtractArc(opts.Input, opts.Output, opts.Overwrite);
                        break;
                    }

                    default:
                    {
                        Console.WriteLine("Unknown file extension.");
                        return;
                    }
                }
            }
        }

        private static void ConvertToBMFontConfig(string input, string output, bool overwrite)
        {
            Console.WriteLine("Converting FNT to BMFC");

            if (File.Exists(output))
            {
                if (!overwrite)
                {
                    Console.Write("Output file already exists. Overwrite? (y/N) ");
                    string result = Console.ReadLine();
                    if (string.IsNullOrEmpty(result) || result.ToUpperInvariant() != "Y")
                    {
                        Console.WriteLine("Cancelled by user!");
                        return;
                    }
                }

                File.Delete(output);
            }

            Node n = NodeFactory.FromFile(input);
            n.TransformWith<FntReader>();
            n.TransformWith<ConfigWriter>();
            n.Stream.WriteTo(output);
        }

        private static void ConvertToDDS(string input, string output, bool overwrite)
        {
            Console.WriteLine("Converting NHTEX to DDS");

            if (File.Exists(output))
            {
                if (!overwrite)
                {
                    Console.Write("Output file already exists. Overwrite? (y/N) ");
                    string result = Console.ReadLine();
                    if (string.IsNullOrEmpty(result) || result.ToUpperInvariant() != "Y")
                    {
                        Console.WriteLine("Cancelled by user!");
                        return;
                    }
                }

                File.Delete(output);
            }

            Node n = NodeFactory.FromFile(input);
            n.TransformWith<NhtexToDds>();
            n.Stream.WriteTo(output);
        }

        private static void ConvertToNhtex(string input, string output, bool overwrite)
        {
            Console.WriteLine("Converting TGA to NHTEX");

            if (File.Exists(output))
            {
                if (!overwrite)
                {
                    Console.Write("Output file already exists. Overwrite? (y/N) ");
                    string result = Console.ReadLine();
                    if (string.IsNullOrEmpty(result) || result.ToUpperInvariant() != "Y")
                    {
                        Console.WriteLine("Cancelled by user!");
                        return;
                    }
                }

                File.Delete(output);
            }

            Node n = NodeFactory.FromFile(input);
            n.TransformWith<TgaToNhtex>();
            n.Stream.WriteTo(output);
        }

        private static void ConvertToPo(string input, string output, bool overwrite, string extension)
        {
            Console.WriteLine($"Converting {extension} to PO");

            if (File.Exists(output))
            {
                if (!overwrite)
                {
                    Console.Write("Output file already exists. Overwrite? (y/N) ");
                    string result = Console.ReadLine();
                    if (string.IsNullOrEmpty(result) || result.ToUpperInvariant() != "Y")
                    {
                        Console.WriteLine("Cancelled by user!");
                        return;
                    }
                }

                File.Delete(output);
            }

            Node n = NodeFactory.FromFile(input);
            var parameters = new CsvParameters
            {
                FileExtension = extension,
            };

            switch (extension)
            {
                case ".csvtxt":
                case ".csvq":
                {
                    parameters.TranslationFields = "English";
                    break;
                }

                case ".csvcr":
                {
                    parameters.TranslationFields = "PostEn,NameEn";
                    break;
                }

                case ".csvwl":
                {
                    parameters.TranslationFields = "NameEn,CommentEn,Floor_NameEn";
                    break;
                }

                default:
                    break;
            }

            n.TransformWith<CsvToPo, CsvParameters>(parameters);
            n.TransformWith<Po2Binary>();
            n.Stream.WriteTo(output);
        }

        private static void ConvertToCsv(string input, string output, bool overwrite)
        {
            Node n = NodeFactory.FromFile(input);
            n.TransformWith<Binary2Po>();

            Po po = n.GetFormatAs<Po>();

            var extension = ".csvtxt";
            if (po.Header.Extensions.ContainsKey("FileExtension"))
            {
                extension = po.Header.Extensions["FileExtension"];
            }

            Console.WriteLine($"Converting PO to {extension}");

            if (string.IsNullOrEmpty(output))
            {
                output = input.Replace(".po", extension);
            }

            if (File.Exists(output))
            {
                if (!overwrite)
                {
                    Console.Write("Output file already exists. Overwrite? (y/N) ");
                    string result = Console.ReadLine();
                    if (string.IsNullOrEmpty(result) || result.ToUpperInvariant() != "Y")
                    {
                        Console.WriteLine("Cancelled by user!");
                        return;
                    }
                }

                File.Delete(output);
            }

            n.TransformWith<PoToCsv>();
            n.Stream.WriteTo(output);
        }

        private static void ExtractArc(string input, string output, bool overwrite)
        {
            Console.WriteLine("Extracting ARC");

            if (Directory.Exists(output) && !overwrite)
            {
                Console.Write("Output directory already exists. Overwrite files? (y/N) ");
                string result = Console.ReadLine();
                if (string.IsNullOrEmpty(result) || result.ToUpperInvariant() != "Y")
                {
                    Console.WriteLine("Cancelled by user!");
                    return;
                }
            }

            Node n = NodeFactory.FromFile(input);
            n.TransformWith<ArcDecompress>();
            n.TransformWith<ArcReader>();

            foreach (Node node in Navigator.IterateNodes(n))
            {
                if (node.IsContainer)
                {
                    continue;
                }

                string path = node.Path.Substring(n.Path.Length);
                var outputPath = string.Concat(output, path);
                node.Stream.WriteTo(outputPath);

                outputPath = string.Concat(outputPath, ".arcinfo");
                string[] info =
                {
                    $"Unknown1={node.Tags["Unknown1"]}",
                    $"Unknown2={node.Tags["Unknown2"]}",
                };

                File.WriteAllLines(outputPath, info);
            }
        }

        private static void BuildArc(string input, string output, bool overwrite)
        {
            Console.WriteLine("Building ARC");

            if (File.Exists(output))
            {
                if (!overwrite)
                {
                    Console.Write("Output file already exists. Overwrite? (y/N) ");
                    string result = Console.ReadLine();
                    if (string.IsNullOrEmpty(result) || result.ToUpperInvariant() != "Y")
                    {
                        Console.WriteLine("Cancelled by user!");
                        return;
                    }
                }

                File.Delete(output);
            }

            Node n = NodeFactory.FromDirectory(input, "*", "Root", true);
            n.SortChildren(
                (x, y) =>
                {
                    if (x.IsContainer && !y.IsContainer)
                    {
                        return -1;
                    }

                    if (!x.IsContainer && y.IsContainer)
                    {
                        return 1;
                    }

                    return string.CompareOrdinal(x.Name, y.Name);
                },
                true);
            n.TransformWith<ArcWriter>();
            n.TransformWith<ArcCompress>();
            n.Stream.WriteTo(output);
        }

        [SuppressMessage(
            "ReSharper",
            "CA1812:ClassNeverInstantiated.Local",
            Justification = "Instantiated by reflection.")]
        private class Options
        {
            [Value(0, Required = true, HelpText = "Input file or directory.")]
            public string Input { get; set; }

            [Value(1, Required = false, HelpText = "Output file or directory.")]
            public string Output { get; set; }

            [Option('o', "overwrite", HelpText = "Set to overwrite without asking.")]
            public bool Overwrite { get; set; }
        }
    }
}
