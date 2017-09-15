Clang Power Tools
=================

A tool bringing clang-tidy magic to Visual Studio C++ developers.

This Visual Studio extension helps Visual Studio C++ developers leverage Clang/LLVM tools like clang-tidy in order to perform various code transformations and fixes like modernizing code to C++11/14/17 and finding subtle latent bugs with its static analyzer and ‘cppcoreguidelines’ modules (Guidelines Support Library).

A list of available checks that clang-tidy can perform:
https://clang.llvm.org/extra/clang-tidy/checks/list.html

The extension can be configured via the standard Visual Studio options panel.
[Tools] > [Options] > [Clang Power Tools]
You can customize the tool behavior as well as the clang++ compilation flags, clang-tidy checks, etc.

Requires: "Clang for Windows" (LLVM pre-built binary) to be installed on the PC.
http://releases.llvm.org/5.0.0/LLVM-5.0.0-win64.exe
http://releases.llvm.org/download.html