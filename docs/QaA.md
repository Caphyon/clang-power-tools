# Tips & Frequently Asked Questions

### 👉 Does Clang Power Tools support `clang-format` ?

Yes. Starting with version 3.0 Clang Power Tools VS extension has built-in support for **[clang-format](https://clang.llvm.org/docs/ClangFormat.html)**  
Auto-format source file on save, configurable [style options](https://clang.llvm.org/docs/ClangFormatStyleOptions.html), file extension rules, etc.

### 👉 What Clang/LLVM tools are supported by Clang Power Tools workflows ?

 * `clang++`      - Clang compile (syntax-only)
 * `clang-tidy`   - Clang Tidy (checks, auto-fixes, modernizing code, static analysis)
 * `clang-format` - source code formating (auto format on save)

### 👉 Do I need to install LLVM for Clang Power Tools to work ?

Yes. 
Download and install **Clang for Windows** (LLVM pre-built binary)  
[releases.llvm.org/download.html](http://releases.llvm.org/download.html)  
Eg. [LLVM-7.0.1-win64](http://releases.llvm.org/7.0.1/LLVM-7.0.1-win64.exe)  

We will automatically load clang from the default installation path *C:\Program Files\LLVM*. If you prefer to use a different location you must manually add the **bin** folder to **PATH**.  

### 👉 What versions of LLVM are supported by Clang Power Tools ?

We tested Clang Power Tools with LLVM `3.9.x`, `4.0.x`, `5.0.x`, `6.0.x` and `7.0.x`

### 👉 How to deal with warnings from system/third-party headers ?

Until v2.6, we've used the `%INCLUDE%` environment variable for setting clang include directories.
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

### 💡 Did you know you can automatically run Clang compile on the current source file after each MSVC compilation ?

This option was a real game changer for our team, because developers often broke CI build pipelines (_Clang_ build). 
_Scenario:_  
Developer compiles code in Visual Studio (`/W4 /WX`) ✔️ ... push commit ... CI build pipeline .... broken **Clang** build 🔥  
After enabling this option (from _Settings_), developers _**immediately**_ see when they compile a source file from VS if it also works in Clang. Each successful MSVC **compile** is automatically followed by a Clang compile. 

### 👉 Where can I see detailed information about clang-tidy checks ?

A list of available checks that `clang-tidy` can perform:  
[clang.llvm.org/extra/clang-tidy/checks/list](https://clang.llvm.org/extra/clang-tidy/checks/list.html)  

### 💡 Did you know you can auto tidy code on save ?

There is a setting (off by default) that enables _automatic_ `clang-tidy` (fix) when the current source file is saved.

### 💡 Did you know you can UNDO all the changes performed by clang-tidy ?

After performing `clang-tidy` on a source file **opened** in the VS editor, you can Undo the tidy changes (atomically) by hitting `Ctrl+Z`

### 👉 How can I use Clang Static Analyzer ?

Clang Static Analyzer was included into `clang-tidy` some time ago.
As a result, you can use Clang Power Tools to run all static analyzer checks from Clang.
Check our extension settings panel, in the Tidy sub-section, scroll to see and ENABLE all the checks starting with: "clang-analyzer-"

More details here:  
[clang-analyzer.llvm.org/available_checks](https://clang-analyzer.llvm.org/available_checks.html)

### 👉 Does Clang Power Tools support automatic checking of CppCoreGuidelines ?

Yes. By leveraging `clang-tidy` support for checking [CppCoreGuidelines](https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md).  
You can use the `cppcoreguidelines-*` filter from Clang Power Tools _settings_, to select CppCoreGuidelines from the available clang-tidy [checks](https://clang.llvm.org/extra/clang-tidy/checks/list.html).

### 👉 If I want to use a .clang-tidy file, where do I put that configuration file on the file system?

`clang-tidy` searches for the config file starting from the given/input source file directory, going up .. until it finds a .clang-tidy file (stops at drive root). This is the standard clang-tidy lookup mechanism.  
FYI, Clang Power Tools has a setting related to this workflow:  
VS Options > Clang Power Tools > Tidy > Options > Use checks from: (combo-box)

### 👉 How do I configure options for specific clang-tidy checks ?

Configuration options for specific clang-tidy checks can be specified via the `.clang-tidy` configuration file.  
Eg.  

    Checks:          '-*,some-check'  
    WarningsAsErrors: ''  
    HeaderFilterRegex: '.*'  
    FormatStyle:     none  
    CheckOptions:  
      - key:             some-check.SomeOption  
        value:           'some value'  
    ...
 

 
