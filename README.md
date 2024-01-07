Tools for Yakuza PAR archives.

# ParLibrary
ParLibrary is a .NET library for reading and writing Yakuza PAR archives.

It supports ***SLLZ*** compression (Including ***SLLZ V2*** used in Yakuza Kiwami 2)

# ParTool

## Usage
- **List mode**

  `ParTool.exe list <archive.par> [-r] [--filter '<regex filter>']`
  
  Reads a PAR archive and shows it contents.
  
  `-r` parameter enables *recursive* mode and shows the contents of nested PAR archives.

  `--filter` parameter filters output lines using a user-provided regular expression.

  For example, `ParTool.exe list mesh.par -r --filter '\.gmd$'` will only list GMD files (files that end with the characters '.gmd').
  See [https://regexr.com/7q33s] for an explanation of this regular expression, and [the .NET Regular Expression Quick Reference](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference) for a complete guide to the syntax.
  
  If using Powershell, use single quotes `'` to surround the expression to avoid variable expansion [(see the Powershell docs)](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_quoting_rules?view=powershell-7.4#single-quoted-strings).
  If using Command Prompt, use double quotes.

- **Extraction mode**

  `ParTool.exe extract <archive.par> <output_directory> [-r] [--filter '<regex filter>']`
  
  Extracts the PAR archive contents to the specified directory.
  
  `-r` parameter enables *recursive* mode and extracts the contents of nested PAR archives.

  `--filter` parameter filters extracted files using a user-provided regular expression.
  Files that do not match the filter will not be extracted.
  Directories are always extracted.
  When combined with `-r`, files matching the filter inside nested PAR archives will be extracted.

  For example, `ParTool.exe extract mesh.par -r --filter '\.gmd$'` will only extract GMD files (files that end with the characters '.gmd').
  See [https://regexr.com/7q33s] for an explanation of this regular expression, and [the .NET Regular Expression Quick Reference](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference) for a complete guide to the syntax.

  If using Powershell, use single quotes `'` to surround the expression to avoid variable expansion [(see the Powershell docs)](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_quoting_rules?view=powershell-7.4#single-quoted-strings).
  If using Command Prompt, use double quotes.

- **Creation mode**

  `ParTool.exe create <input_directory> <archive.par> [-c compression_mode] [--alternative-mode]`
  
  Creates a new PAR archive with the contents of the specified directory.
  
  `-c` parameter sets the SLLZ compression version to use. 
    - `0` Don't compress. It is faster but files will be larger.
    - `1` is the default value. It is supported in all Yakuza games.
    - `2` is only supported in Yakuza Kiwami 2.
    
  Set `--alternative-mode` if the output file will be used in Yakuza 3, 4, 5 or Kenzan.

- **Delete mode**

  `ParTool.exe remove <original.par> <node_to_delete> <new.par>`
  
  Reads a PAR archive and creates a new one without the specified file or folder.
  
- **Add files mode**

  `ParTool.exe add <original.par> <input_directory> <new.par> [-c compression_mode]`
  
  Reads a PAR archive and creates a new one adding the files and folders located in the specified directory.
 
  `-c` parameter sets the SLLZ compression version to use (only added files will use it). 
    - `0` Don't compress. It is faster but files will be larger.
    - `1` is the default value. It is supported in all Yakuza games.
    - `2` is only supported in Yakuza Kiwami 2.

- **Drag & Drop**
  
  Since v1.2.0, you can drag & drop a file or a folder on the exe. 
  If you drop a PAR archive, the application will extract it in the same location of the archive.
  If you drop a folder, the application will create a PAR archive with `-c 1` parameter.

# Credits
* Thanks to Pleonex for [Yarhl](https://scenegate.github.io/Yarhl/).
* Thanks to Rick Gibbed for the SLLZ decompression algorithm.
