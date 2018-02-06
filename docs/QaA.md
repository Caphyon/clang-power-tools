# Frequently Asked Questions

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

### 👉 How can I use Clang Static Analyzer ?

Clang Static Analyzer was included into `clang-tidy` some time ago.
As a result, you can use Clang Power Tools to run all static analyzer checks from Clang.
Check our extension settings panel, in the Tidy sub-section, scroll to see and ENABLE all the checks starting with: "clang-analyzer-"

More details here:  
[clang-analyzer.llvm.org/available_checks](https://clang-analyzer.llvm.org/available_checks.html)

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
 
### 👉 Do I need to install LLVM for Clang Power Tools to work ?

Yes. 
Download and install **Clang for Windows** (LLVM pre-built binary)  
[LLVM-5.0.1-win64](http://releases.llvm.org/5.0.1/LLVM-5.0.1-win64.exe)  
[releases.llvm.org/download.html](http://releases.llvm.org/download.html)

We will automatically load clang from the default installation path *C:\Program Files\LLVM*. If you prefer to use a different location you must manually add the **bin** folder to **PATH**.  

 
