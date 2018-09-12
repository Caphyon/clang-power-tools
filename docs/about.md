# Clang Power Tools (for Visual Studio)

Clang Power Tools is a **free** _open-source_ Visual Studio extension helping Visual Studio C++ developers leverage Clang/LLVM tools (clang++, clang-tidy and clang-format), in order to perform various code transformations and fixes like modernizing code to C++ 11 / 14 / 17 and finding subtle latent bugs with its static analyzer and C++ Core Guidelines checks.

This project debuted at CppCon 2017, when we introduced the recently open-sourced project to the C++ community.  
https://www.youtube.com/watch?v=Wl-9ozmxXbo  

## Motivation

C++ has an open, collaborative, ever evolving community. It also has a convoluted web of language specifications, standard libraries, compilers vendors and IDEs. This decade has brought us major developments, C++ 11 / 14 / 17 make use of modern language and library features to better our production code, improve safety, performance and maintainability.

While the standard committee has been busy refining new proposals and improving the language standard, the software development industry has been busy as well. We've seen progress from major compiler vendors: Clang/LLVM, GCC, and MSVC. C++ 17 standard conformance is mostly complete across all three.

But, newly written code should not be the only modern code in a codebase. As a language with decades of extant production code, C++ presents the community with a challenge: modernize large amounts of existing code. Clang/LLVM has risen up to the challenge by offering an extensive array of tools for this purpose: static analysis, modernizers, automatic formatting. The catch, however, is that these tools are not easily accessible on all platforms.

Windows has traditionally been a MSVC-only world. Microsoft has made great strides in improving this in recent years, first with the now defunct Clang/C2 in 2016 and then with ensuring VC++ STL libraries compile against Clang/LLVM. In early 2017, a small group of Caphyon developers volunteered for an internal effort to modernize our 15 year old code-base, consisting of roughly 3 million lines of C++ code. We had gotten inspired to do this by CppCon 2016 talks about Clang/LLVM and its modernizers. 

Working with basically only clang++/clang-tidy and a command line console we saw an opportunity in all this: bringing the power of Clang / LLVM infrastructure directly into Visual Studio, as a an extension that offers painless, seamless integration into existing Visual Studio projects, without messing with command lines, manual invocations and other kinds of dirty voodoo. 

The end goal: help Windows developers in their efforts to modernize their existing C++ code.

## Project progress

In the early days of Spring 2017, Clang Power Tools was simply a Powershell script which we invoked with various flags that would invoke clang / clang-tidy on our projects. As the project progressed, it became more and more popular internally and the need for a more easy-to-use tool quickly arose. This led to the creation of our Visual Studio extension, bringing the modernization workflow directly into Visual Studio IDE.

After the VS Extension effort began, we open sourced the project and soon afterwards it had its debut at CppCon 2017. Development has ramped up since then, with major improvements in both the backend and user interface.

Naturally, Powershell integration remains an important part for continuous automation purposes. Internally, we're using Jenkins Clang Power Tools jobs which help us keep code quality up, on a daily-development basis.

The project growth has been made possible by great feedback from our Github users. We've continuously worked on enhancing performance and adding support for more flavors of Visual Studio project configurations. Since its debut at Cppcon, we've added features like auto-triggered compilation, support for clang-format and header-triggered translation unit compilation, a fresh WPF user interface, and more.

With the help of its growing community, Clang Power Tools is only ever going to get better.
