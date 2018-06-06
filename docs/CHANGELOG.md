# Version History

### Clang Power Tools 3.5
*June 5, 2018*

Improvements:

* Support for compiling `C files`

Bugs:

* The `File Ignore` and `Project Ignore` filters didn't worked
* `Clang Compile` and `Clang Tidy` commands crashed when extra white spaces and extra delimitaters were added in UI text fields
* Buttons have overlapped the text in the UI fields

### Clang Power Tools 3.4.2
*May 15, 2018*

Bugs:

* Selecting elements from the `Tidy` -> `Predefined Checks` page has caused crashes.

### Clang Power Tools 3.4.1
*May 15, 2018*

Bugs:

* Auto clang-format on save did not work for upgraded installs

### Clang Power Tools 3.4
*May 14, 2018*

Improvements:

* Updated "clang-format.exe" to the latest version 6.0.0
* Support to use different version of "clang-format.exe" from the one in the box
* Redesigned settings panel

Bugs:

* Errors in headers were shown multiple times if different folder delimiters were used.

### Clang Power Tools 3.3.1
*April 24, 2018*

Bugs:

* Clang output text was separated into multiple lines.
* Source files with ExcludedFromBuild child property set to true were compiled.
* Empty project files were not correctly handled.

### Clang Power Tools 3.3
*March 29, 2018*

Improvements:

* Improved responsiveness during lengthy operations.
* Added support for filter errors by "Project" field in the Error List.
* Cleanup errors when closing a project.
* Use the whole file name of PCH cpp to detect PCH header directory.

### Clang Power Tools 3.2.1
*March 23, 2018*

Bugs:

* Clang compile fails when project has no Additional Include Directories.
* Error line number from error list was displayed wrong.

### Clang Power Tools 3.2
*March 22, 2018*

Improvements:

* Added support for VS status bar.
* Added clang-tidy checker to output window messages.

Bugs:

* Last error message couldn't be detected.
* Consecutive error messages couldn't be detected. 
* Did not work on Visual Studio 2017 Update 2 because vswhere does not support the -prerelease switch.
* Did not work when stdafx.cpp contained commented #include lines.
* Couldn't detect default Configuration Platform if comment nodes were present in the `<ItemGroup>` XML element.
* PCH header couldn't be detected when in a different physical location than that of PCH cpp.
* PCH header couldn't be detected if not present in project `<ClInclude>` XML element.
* Compilation stopped on PCH-creation errors even if -continue switch was present.

### Clang Power Tools 3.1
*March 12, 2018*

Improvements:

* Added support for [MSBuild]::GetDirectoryNameOfFileAbove.
* Added support for clang format selection.

Bugs:

* /MD and /MT project settings not propagated to clang

### Clang Power Tools 3.0.1
*March 2nd, 2018*

Bugs:

* Visual Studio loses focus on selected items.

### Clang Power Tools 3.0
*March 1st, 2018*

**NEW**: Built-in support for **[clang-format](https://clang.llvm.org/docs/ClangFormat.html)**  
Auto-format source file on save, configurable [style options](https://clang.llvm.org/docs/ClangFormatStyleOptions.html), file extension rules, etc.

Improvements:

* Disable the clang commands when VS build is running.

Bugs:

* Automatic clang-tidy on save option was applying fixes even when Fix option was set to _false_.
* Running a command with different elements of VS selected (eg. properties window, toolbox) sometimes caused errors.
* Clang compile and tidy commands were not disabled for source files not loaded in a project context.
* Properly trim whitespace from #include paths.
* Whitespaces in VS project name were not supported.

### Clang Power Tools 2.8
*February 20, 2018*

Improvements:

* Automatically run Clang-tidy when the current source file is saved.
* Automatically run Clang compile on the current source file after successful MSVC compilation.

Bugs:

* A project was saved even if it was not dirty.
* Stop Clang command crashed all Powershell processes.
* A single error found in a header was displayed multiple times.
* COM objects caused errors when a command was running.


### Clang Power Tools 2.7
*February 8, 2018*

Improvements:
* Save all projects before running clang compile/tidy.

Bugs:
* The `Treat additional includes as` option was not loaded from the settings file.
* The toolbar was hidden on the first install.
* Source files / headers not detected when specified in property sheets.

### Clang Power Tools 2.6
*January 29, 2018*

Until now, we've used the `%INCLUDE%` environment variable for setting clang include directories.
That was equivalent to using `-isystem` for each directory. 
   
Unfortunately, this caused headers included relative to those include directories to be ignored 
when running **compiling/tidying** because they were treated as **system headers**. 
   
Having this brought to our attention, going forward we will use `-I` and `-isystem` to pass include 
directories to **clang**, with the following defaults:
   * include directories            passed using `-isystem`
   * additional include directories passed using `-I`
   
   Q: What does this mean?    
   A: You'll most likely see **new warnings** when **compiling** or **tidying** your code-base.
   
   Q: Will my build fail?       
   A: Only if you have specified `-Werror` (treat warnings as errors).
   
   Q: What should I do?     
   A: - Make sure to include third party library dependencies via the **Include directories** project option.
      **Additional include directories** should point only to code you can modernize.     
      - Resolve remaining warnings related to your code.
   
   Q: Can I use **ClangPowerTools** using the **old behavior**?     
   A: Yes. We've added, for compatibility reasons, a **UI option** that allows clang to **treat 
      additional includes as system headers**. Keep in mind this means we will potentially 
      miss some of your headers when calling clang.
     
   You may want to use this option if using `-Werror` (treating warnings as errors) until you've 
   reorganized your includes, since any new warnings will break your build.
     
   Q: What about the **continuous integration script** (clang-build.ps1)?     
   A: You can specify the `-treat-sai` switch and it will have the old behavior.

Improvements:
* Additional includes are now treated as regular includes using `-I`

Bugs:
* Crash when using preprocessor definitions containing double quotes
* Wild card project file includes were not recognized
* Tooling detection did not work on Visual Studio pre-release versions

### Clang Power Tools 2.5.1
*January 17, 2018*

Bugs:
* Fixed the unexpectedly Visual Studio session ending.

### Clang Power Tools 2.5
*January 12, 2018*

Improvements:
* Automatically show toolbar on the first time install.

Bugs:
* Fixed errors detection.
* Fixed commands execution when the project contains more files with the same name.
* Crash when multiple SKUs of Visual Studio 2017 installed simultaneously.
* Google.Test Nuget packages not working.

### Clang Power Tools 2.4.1
*December 28, 2017*

Bugs:
* Regression in 2.4.0 preventing use of clang-tidy on header files.

### Clang Power Tools 2.4
*December 21, 2017*

Improvements:
* Configurable header-filter for tidy-fix.
* Option to run clang-tidy only on cpp-corresponding headers.

Bugs:
* `<Choose>` project elements were not loaded.
* Same-file appended project settings caused crash.
* Wildcard-named property sheets were not loaded.

### Clang Power Tools 2.3
*December 13, 2017*

Improvements:
* Tidy operation mode: ability to switch between explicitly specified tidy checks and .clang-tidy configuration files.
* Use PCH for clang-tidy workflow.

Bugs:
* Could not property detect C++17 standard from project.
* Project parse issues for MsBuild string member function calls.
* Files with extensions other than .cpp were not compiled from VS.
* `HasTrailingSlash` MsBuild function was not recognized.

### Clang Power Tools 2.2
*December 11, 2017*

Bugs:
* Fixed the saving for user configurations from option pages.
* Increased the number of compilable file types. Now supporting: .c , .cpp , .cc , .cxx , .c++ , .cp
* Could not be used in VS 2015 (regression in v2.1).
* Could not create PCH when using forced includes and stdafx.cpp did not manually include stdafx.h.
* Could not handle multiple MSVC versions found in the same VS installation.
* Project load error when a project condition contained more than one call to MSBuild `Exists()`.
* Clang-tidy `-header-filter` flag was sometimes behaving incorrectly, ignored header files.

### Clang Power Tools 2.1
*December 7, 2017*

Improvements:
* Possibility to abort clang compile and clang tidy commands.
* Eliminated delay caused by .sln and .vcxproj file scanning.
* Create PCH only for more than 2 cpps.
* Detect C++ standard automatically from project. Default to C++14 if not set.

Bugs:
* Environment variables were not used when evaluating MsBuild expressions.
* MsBuild expressions didn't work when starting with ! operator.
* ForcedIncludes for clang compile were not working, regression in v2.0.
* ForcedIncludes were not taken into account for clang-tidy.
* Project > IncludePath was not taken into account for include directories.
* Error occured when include directories contained empty values.
* Fixed PCH crash when using Visual Studio 15.5 STL libraries (mscver 14.12.25827).
* Fixed the commands execution for default console application.

### Clang Power Tools 2.0
*November 26, 2017*

Improvements:
* Fully evaluate MsBuild conditions for project and property sheet XML data.
  * XML nodes with conditions evaluated to false are ignored.
* Project settings can now reference:
  * User defined macros 
    * e.g. $(MY_MACRO_DIR)\bin\$(MY_MACRO_FILE)
  * Build-in macros: 
    * $(VisualStudioVersion), $(ProjectDir), $(SolutionDir), $(Platform), $(Configuration), $(MSBuildProjectName), $(MSBuildProjectFullPath), $(MSBuildProjectDirectory), $(MSBuildThisFile), $(MSBuildThisFileName), $(MSBuildThisFileFullPath), $(MSBuildThisFileDirectory), $(MSBuildProgramFiles32).
  * .NET expressions
    * e.g. $([System.DateTime]::Now.ToString("yyyy.MM.dd"))
* Detect property sheets included in Directory.Build.props.
* Automatically detect the project solution file.
    
### Clang Power Tools 1.7.1
*November 20, 2017*

Bugs:
* Fixed the support for `Track Active Item in Solution Explorer` set to `Off`.

### Clang Power Tools 1.7
*November 16, 2017*

Improvements:
* Added support for `Track Active Item in Solution Explorer` option set to `Off`.
* Verbose-Mode now logs `clang-build.ps1` and `clang++` invocation arguments.

Bugs:
* Fixed the `user settings saving`.
* Fixed the script execution for clang-tidy options.
* Moved `Clang Power Tools` above the `Properties` item in `Context Menu`.


### Clang Power Tools 1.6
*November 9, 2017*

Improvements:
* Improved handling of project value separator.
* Used the active project configuration.

Bugs:
* Fixed the commands launching from file tab.
* Fixed the default tidy settings page.
* Fixed the cleaning of all messages from Error tab when a command begins.

### Clang Power Tools 1.5.1
*November 3rd, 2017*

Improvements:
* Created dedicated option page for tidy checks.
* Provided better defaults for "Tidy Checks" dialog.

Bugs:
* Fixed script building for tidy checks. 

### Clang Power Tools 1.5
*November 1st, 2017*

Improvements:
* Generalized property sheet usage for project data retrieval.
* Detect auto property sheets ([Directory.Build.props](https://docs.microsoft.com/en-us/visualstudio/msbuild/customize-your-build)). Merging not supported.
* Detect property sheets included indirectly, through other property sheets.
* Automatically detect the first project configuration platform.

### Clang Power Tools 1.4
*October 27, 2017*

Improvements:
* Added verbose mode in settings.
* Created Clang Power Tools toolbar.

Bugs:
* Fixed logical core count detection on systems with more than one physical CPU.
* Disabled the commands when another command is running.
* Fixed MSCVER detection bug when VS is not installed in default location.
* Added WinSDKVer defaults for when not mentioned in project.
* Fixed error message when project does not include force include files.

### Clang Power Tools 1.3.1
*October 23, 2017*

Bugs:
* Allow the path to include any non-whitespace character.
* Added support for project-specific force include files.
* Added support for Win 8/8.1 SDKs.
* Fix typo preventing detection of VS2015 install location.
* Fixed crash when project has no files to compile.

### Clang Power Tools 1.3
*October 19, 2017*

Improvements:
* Added support for ignoring specific files.
* Added support for treating warnings as errors.
* Detect Visual Studio 2015 / 2017 custom installation paths.
* Enabled compilation for all files included in the Vcxproj <ClCompile> section.

Bugs:
* Fixed project detection when the project name from UI is different of the project name on the disk.
* Prevents the Output window from stealing the focus.
* Fixed the detection for modified files after clang tidy fix.
* Fixed the "Project To Ignore" option.

### Clang Power Tools 1.2
*October 13, 2017*

Improvements:
* Added clang warnings and messages with line navigation in both output pane and error list.
* Included clang notification(errors, warnings, messages) in Build Only category.
* Formatted clang output.

Bugs:
* Fixed clang error detection algorithm.
* Fixed clang errors line navigation from output pane.
* Fixed the vsix installer detection for VS2017.

### Clang Power Tools 1.1
*October 8, 2017*

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
*September 30, 2017*

Improvements:
* Automatically detect clang installation folder.
* Support for projects without PCH/stdafx.

Bugs:
* Fixed navigation to clang error in code editor.
* Fixed clang error parse algorithm.
* Fixed detection when lightweight solution load is enabled.

### Clang Power Tools 1.0
*September 27, 2017*

First official release.



