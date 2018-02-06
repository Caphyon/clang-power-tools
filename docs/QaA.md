# Q & A

## How to deal with warnings from system/third-party headers ?

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

