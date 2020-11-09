Tools for Yakuza PAR archives.

# ParLibrary
ParLibrary is a .NET library for reading and writing Yakuza PAR archives.

It supports ***SLLZ*** compression (Including ***SLLZ V2*** used in Yakuza Kiwami 2)

# ParTool

## Usage
- **List mode**

  `ParTool.exe list <archive.par> [-r]`
  
  Reads a PAR archive and shows it contents.
  
  `-r` parameter enables *recursive* mode and shows the contents of nested PAR archives.
  
- **Extraction mode**

  `ParTool.exe extract <archive.par> <output_directory> [-r]`
  
  Extracts the PAR archive contents to the specified directory.
  
  `-r` parameter enables *recursive* mode and extracts the contents of nested PAR archives.
  
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
