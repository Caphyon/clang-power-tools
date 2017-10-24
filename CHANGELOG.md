# ChangeLog

Changelog of Clang Power Tools extension

### Clang Power Tools 1.3.1

Bugs:
* Allow the path to include any non-whitespace character.
* Added support for project-specific force include files.
* Added support for Win 8/8.1 SDKs.
* Fix typo preventing detection of VS2015 install location.
* Fixed crash when project has no files to compile.

### Clang Power Tools 1.3.0

Improvements:
* Added support for ignoring specific files
* Added support for treating warnings as errors
* Detect Visual Studio 2015 / 2017 custom installation paths
* Enabled compilation for all files included in the Vcxproj <ClCompile> section

Bugs:
* Fixed project detection when the project name from UI is different of the project name on the disk.
* Prevents the Output window from stealing the focus.
* Fixed the detection for modified files after clang tidy fix.
* Fixed the "Project To Ignore" option

### Clang Power Tools 1.2.0

Improvements:
* Added clang warnings and messages with line navigation in both output pane and error list.
* Included clang notification(errors, warnings, messages) in Build Only category.
* Formatted clang output.

Bugs:
* Fixed clang error detection algorithm.
* Fixed clang errors line navigation from output pane.
* Fixed the vsix installer detection for VS2017.

### Clang Power Tools 1.1.0

Improvements:
* Support for Visual Studio 2015.
* Open the modified files after clang tidy format command.
* Suppress the reload popup shown by the Visual Studio.
* Integrated -quiet in the same way as other clag flags.
* Generalized PCH support for more than stdafx.h.

Bugs:
* Clean the output and errors before a new command and build action.
* Fixed the include directories option.
* Fixed error detection for error list.
* Fixed LLVM detection mechanism from Visual Studio Extension.
* Switched to -fix-errors when calling clang-tidy.
* Removed restriction to CPP files when looking for ClCompile entries.
* Fixed compilation when vcxproj has both 32 and 64 bit platform configrations.

### Clang Power Tools 1.0.1

Improvements:
* Automatically detect clang installation folder.
* Support for projects without PCH/stdafx.

Bugs:
* Fixed navigation to clang error in code editor.
* Fixed clang error parse algorithm.
* Fixed detection when lightweight solution load is enabled.

### Clang Power Tools 1.0.0

First official release.



