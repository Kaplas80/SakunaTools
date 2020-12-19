# Sakuna: Of Rice And Ruins Tools

Tools for translating **Sakuna: Of Rice And Ruins**

## Download

You can download it from the [Releases](https://github.com/Kaplas80/SakunaTools/releases) section.

## Usage

### Basic

Drag & drop a file onto the application exe.

### Advanced

Open a command prompt and run:
`SakunaTools.exe <input> <output> [-o|--overwrite] [--switch]`

## Valid inputs

### ARC Container

If the input is a `.arc` container, the app will extract it.

### Directory

If the input is a directory, the app will create a `.arc` container with its files.

### Font file

If the input is a `.fnt` file, the app will create a [BMFont](https://www.angelcode.com/products/bmfont/) config file to create a new font with the same parameters.

### NHTEX font image

If the input is a `.nhtex` font file, the app will create a DDS image.

NOTE: If the file is from the Nintendo Switch game, you have to use `--switch` to run the app.

### TGA font image

If the input is a `.tga` image generated with [BMFont](https://www.angelcode.com/products/bmfont/), the app will create a `.nhtex` file.

NOTE: If the file is for the Nintendo Switch game, you have to use `--switch` to run the app.

### CSV file

If the input is a `.csvtxt`, `.csvcr`, `.csvq`, `.csvwl` or `.csv`, the app will create a `.po` file with the English texts.

### PO file

If the input is a `.po` file, the app will create a `.csvtxt`, `.csvcr`, `.csvq`, `.csvwl` or `.csv` file to use in the game.

## Credits
* Thanks to Pleonex for [Yarhl](https://scenegate.github.io/Yarhl/).
* Thanks to Megaflan for the NHTEX compression.
* Swizzle algorithm taken from KillzXGaming's [Switch-Toolbox](https://github.com/KillzXGaming/Switch-Toolbox).
