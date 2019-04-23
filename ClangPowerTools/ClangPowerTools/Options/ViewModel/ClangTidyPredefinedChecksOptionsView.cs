using ClangPowerTools.Options.Model;
using ClangPowerTools.Options.View;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace ClangPowerTools
{
  public class ClangTidyPredefinedChecksOptionsView : ConfigurationPage<ClangTidyPredefinedChecksOptions>
  {
    #region Members

    private const string kGeneralSettingsFileName = "TidyPredefinedChecksConfiguration.config";
    private SettingsPathBuilder mSettingsPathBuilder = new SettingsPathBuilder();

    private ClangTidyPredefinedChecksOptionsUserControl mUserControl;

    #endregion

    #region Properties


    [Category("Checks")]
    [DisplayName("abseil-duration-division")]
    [Description("absl::Duration arithmetic works like it does with integers. That means that division of two absl::Duration objects returns an int64 with any fractional component truncated toward 0. See this link for more information on arithmetic with absl::Duration.")]
    [ClangCheck(true)]
    public bool AbseilDurationDivision { get; set; }


    [Category("Checks")]
    [DisplayName("abseil-duration-factory-float")]
    [Description("Checks for cases where the floating-point overloads of various absl::Duration factory functions are called when the more-efficient integer versions could be used instead.")]
    [ClangCheck(true)]
    public bool AbseilDurationFactoryFloat { get; set; }


    [Category("Checks")]
    [DisplayName("abseil-duration-factory-scale")]
    [Description("Checks for cases where arguments to absl::Duration factory functions are scaled internally and could be changed to a different factory function. This check also looks for arguements with a zero value and suggests using absl::ZeroDuration() instead.")]
    [ClangCheck(true)]
    public bool AbseilDurationFactoryScale { get; set; }


    [Category("Checks")]
    [DisplayName("abseil-faster-strsplit-delimiter")]
    [Description("Finds instances of absl::StrSplit() or absl::MaxSplits() where the delimiter is a single character string literal and replaces with a character. The check will offer a suggestion to change the string literal into a character. It will also catch code using absl::ByAnyChar() for just a single character and will transform that into a single character as well.")]
    [ClangCheck(true)]
    public bool AbseilFasterStrsplitDelimiter { get; set; }


    [Category("Checks")]
    [DisplayName("abseil-no-internal-dependencies")]
    [Description("Warns if code using Abseil depends on internal details. If something is in a namespace that includes the word “internal”, code is not allowed to depend upon it beaucse it’s an implementation detail. They cannot friend it, include it, you mention it or refer to it in any way. Doing so violates Abseil’s compatibility guidelines and may result in breakage. See https://abseil.io/about/compatibility for more information.")]
    [ClangCheck(true)]
    public bool AbseilNoInternalDependencies { get; set; }


    [Category("Checks")]
    [DisplayName("abseil-no-namespace")]
    [Description("Ensures code does not open namespace absl as that violates Abseil’s compatibility guidelines. Code should not open namespace absl as that conflicts with Abseil’s compatibility guidelines and may result in breakage.")]
    [ClangCheck(true)]
    public bool AbseilNoNamespace { get; set; }


    [Category("Checks")]
    [DisplayName("abseil-redundant-strcat-calls")]
    [Description("Suggests removal of unnecessary calls to absl::StrCat when the result is being passed to another call to absl::StrCat or absl::StrAppend.")]
    [ClangCheck(true)]
    public bool AbseilRedundantStrcatCalls { get; set; }


    [Category("Checks")]
    [DisplayName("abseil-str-cat-append")]
    [Description("Flags uses of absl::StrCat() to append to a std::string. Suggests absl::StrAppend() should be used instead.")]
    [ClangCheck(true)]
    public bool AbseilStrCatAppend { get; set; }


    [Category("Checks")]
    [DisplayName("abseil-string-find-startswith")]
    [Description("Checks whether a std::string::find() result is compared with 0, and suggests replacing with absl::StartsWith(). This is both a readability and performance issue.")]
    [ClangCheck(true)]
    public bool AbseilStringFindStartswith { get; set; }


    [Category("Checks")]
    [DisplayName("alpha.osx.cocoa.MissingSuperCall")]
    [Description("Warn about Objective-C methods that lack a necessary call to super. (Note: The compiler now has a warning for methods annotated with objc_requires_super attribute. The checker exists to check methods in the Cocoa frameworks that haven't yet adopted this attribute.) ")]
    [ClangCheck(true)]
    public bool AlphaosxcocoaMissingSuperCall { get; set; }


    [Category("Checks")]
    [DisplayName("android-cloexec-accept")]
    [Description("The usage of accept() is not recommended, it’s better to use accept4(). Without this flag, an opened sensitive file descriptor would remain open across a fork+exec to a lower-privileged SELinux domain.")]
    [ClangCheck(true)]
    public bool AndroidCloexecAccept { get; set; }


    [Category("Checks")]
    [DisplayName("android-cloexec-accept4")]
    [Description("accept4() should include SOCK_CLOEXEC in its type argument to avoid the file descriptor leakage. Without this flag, an opened sensitive file would remain open across a fork+exec to a lower-privileged SELinux domain.")]
    [ClangCheck(true)]
    public bool AndroidCloexecAccept4 { get; set; }


    [Category("Checks")]
    [DisplayName("android-cloexec-creat")]
    [Description("The usage of creat() is not recommended, it’s better to use open().")]
    [ClangCheck(true)]
    public bool AndroidCloexecCreat { get; set; }


    [Category("Checks")]
    [DisplayName("android-cloexec-dup")]
    [Description("The usage of dup() is not recommended, it’s better to use fcntl(), which can set the close-on-exec flag. Otherwise, an opened sensitive file would remain open across a fork+exec to a lower-privileged SELinux domain.")]
    [ClangCheck(true)]
    public bool AndroidCloexecDup { get; set; }


    [Category("Checks")]
    [DisplayName("android-cloexec-epoll-create")]
    [Description("The usage of epoll_create() is not recommended, it’s better to use epoll_create1(), which allows close-on-exec.")]
    [ClangCheck(true)]
    public bool AndroidCloexecEpollCreate { get; set; }


    [Category("Checks")]
    [DisplayName("android-cloexec-epoll-create1")]
    [Description("epoll_create1() should include EPOLL_CLOEXEC in its type argument to avoid the file descriptor leakage. Without this flag, an opened sensitive file would remain open across a fork+exec to a lower-privileged SELinux domain.")]
    [ClangCheck(true)]
    public bool AndroidCloexecEpollCreate1 { get; set; }


    [Category("Checks")]
    [DisplayName("android-cloexec-fopen")]
    [Description("fopen() should include e in their mode string; so re would be valid. This is equivalent to having set FD_CLOEXEC on that descriptor.")]
    [ClangCheck(true)]
    public bool AndroidCloexecFopen { get; set; }


    [Category("Checks")]
    [DisplayName("android-cloexec-inotify-init")]
    [Description("The usage of inotify_init() is not recommended, it’s better to use inotify_init1().")]
    [ClangCheck(true)]
    public bool AndroidCloexecInotifyInit { get; set; }


    [Category("Checks")]
    [DisplayName("android-cloexec-inotify-init1")]
    [Description("inotify_init1() should include IN_CLOEXEC in its type argument to avoid the file descriptor leakage. Without this flag, an opened sensitive file would remain open across a fork+exec to a lower-privileged SELinux domain.")]
    [ClangCheck(true)]
    public bool AndroidCloexecInotifyInit1 { get; set; }


    [Category("Checks")]
    [DisplayName("android-cloexec-memfd-create")]
    [Description("memfd_create() should include MFD_CLOEXEC in its type argument to avoid the file descriptor leakage. Without this flag, an opened sensitive file would remain open across a fork+exec to a lower-privileged SELinux domain.")]
    [ClangCheck(true)]
    public bool AndroidCloexecMemfdCreate { get; set; }


    [Category("Checks")]
    [DisplayName("android-cloexec-open")]
    [Description("A common source of security bugs is code that opens a file without using the O_CLOEXEC flag.  Without that flag, an opened sensitive file would remain open across a fork+exec to a lower-privileged SELinux domain, leaking that sensitive data. Open-like functions including open(), openat(), and open64() should include O_CLOEXEC in their flags argument.")]
    [ClangCheck(true)]
    public bool AndroidCloexecOpen { get; set; }


    [Category("Checks")]
    [DisplayName("android-cloexec-socket")]
    [Description("socket() should include SOCK_CLOEXEC in its type argument to avoid the file descriptor leakage. Without this flag, an opened sensitive file would remain open across a fork+exec to a lower-privileged SELinux domain.")]
    [ClangCheck(true)]
    public bool AndroidCloexecSocket { get; set; }


    [Category("Checks")]
    [DisplayName("android-comparison-in-temp-failure-retry")]
    [Description("Diagnoses comparisons that appear to be incorrectly placed in the argument to the TEMP_FAILURE_RETRY macro. Having such a use is incorrect in the vast majority of cases, and will often silently defeat the purpose of the TEMP_FAILURE_RETRY macro.")]
    [ClangCheck(true)]
    public bool AndroidComparisonInTempFailureRetry { get; set; }


    [Category("Checks")]
    [DisplayName("boost-use-to-string")]
    [Description("This check finds conversion from integer type like int to std::string or std::wstring using boost::lexical_cast, and replace it with calls to std::to_string and std::to_wstring.")]
    [ClangCheck(true)]
    public bool BoostUseToString { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-argument-comment")]
    [Description("Checks that argument comments match parameter names.")]
    [ClangCheck(true)]
    public bool BugproneArgumentComment { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-assert-side-effect")]
    [Description("Finds assert() with side effect.")]
    [ClangCheck(true)]
    public bool BugproneAssertSideEffect { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-bool-pointer-implicit-conversion")]
    [Description("Checks for conditions based on implicit conversion from a bool pointer to bool.")]
    [ClangCheck(true)]
    public bool BugproneBoolPointerImplicitConversion { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-copy-constructor-init")]
    [Description("Finds copy constructors where the constructor doesn’t call the copy constructor of the base class.")]
    [ClangCheck(true)]
    public bool BugproneCopyConstructorInit { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-dangling-handle")]
    [Description("Detect dangling references in value handles like std::experimental::string_view. These dangling references can be a result of constructing handles from temporary values, where the temporary is destroyed soon after the handle is created.")]
    [ClangCheck(true)]
    public bool BugproneDanglingHandle { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-exception-escape")]
    [Description("Finds functions which may throw an exception directly or indirectly, but they should not. The functions which should not throw exceptions are the following: * Destructors * Move constructors * Move assignment operators * The main() functions * swap() functions * Functions marked with throw() or noexcept * Other functions given as option")]
    [ClangCheck(true)]
    public bool BugproneExceptionEscape { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-fold-init-type")]
    [Description("The check flags type mismatches in folds like std::accumulate that might result in loss of precision. std::accumulate folds an input range into an initial value using the type of the latter, with operator+ by default. This can cause loss of precision through:")]
    [ClangCheck(true)]
    public bool BugproneFoldInitType { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-forward-declaration-namespace")]
    [Description("Checks if an unused forward declaration is in a wrong namespace.")]
    [ClangCheck(true)]
    public bool BugproneForwardDeclarationNamespace { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-forwarding-reference-overload")]
    [Description("The check looks for perfect forwarding constructors that can hide copy or move constructors. If a non const lvalue reference is passed to the constructor, the forwarding reference parameter will be a better match than the const reference parameter of the copy constructor, so the perfect forwarding constructor will be called, which can be confusing. For detailed description of this issue see: Scott Meyers, Effective Modern C++, Item 26.")]
    [ClangCheck(true)]
    public bool BugproneForwardingReferenceOverload { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-inaccurate-erase")]
    [Description("Checks for inaccurate use of the erase() method.")]
    [ClangCheck(true)]
    public bool BugproneInaccurateErase { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-incorrect-roundings")]
    [Description("Checks the usage of patterns known to produce incorrect rounding. Programmers often use:")]
    [ClangCheck(true)]
    public bool BugproneIncorrectRoundings { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-integer-division")]
    [Description("Finds cases where integer division in a floating point context is likely to cause unintended loss of precision.")]
    [ClangCheck(true)]
    public bool BugproneIntegerDivision { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-lambda-function-name")]
    [Description("Checks for attempts to get the name of a function from within a lambda expression. The name of a lambda is always something like operator(), which is almost never what was intended.")]
    [ClangCheck(true)]
    public bool BugproneLambdaFunctionName { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-macro-parentheses")]
    [Description("Finds macros that can have unexpected behaviour due to missing parentheses.")]
    [ClangCheck(true)]
    public bool BugproneMacroParentheses { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-macro-repeated-side-effects")]
    [Description("Checks for repeated argument with side effects in macros.")]
    [ClangCheck(true)]
    public bool BugproneMacroRepeatedSideEffects { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-misplaced-operator-in-strlen-in-alloc")]
    [Description("Finds cases where 1 is added to the string in the argument to strlen(), strnlen(), strnlen_s(), wcslen(), wcsnlen(), and wcsnlen_s() instead of the result and the value is used as an argument to a memory allocation function (malloc(), calloc(), realloc(), alloca()) or the new[] operator in C++. The check detects error cases even if one of these functions (except the new[] operator) is called by a constant function pointer.  Cases where 1 is added both to the parameter and the result of the strlen()-like function are ignored, as are cases where the whole addition is surrounded by extra parentheses.")]
    [ClangCheck(true)]
    public bool BugproneMisplacedOperatorInStrlenInAlloc { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-misplaced-widening-cast")]
    [Description("This check will warn when there is a cast of a calculation result to a bigger type. If the intention of the cast is to avoid loss of precision then the cast is misplaced, and there can be loss of precision. Otherwise the cast is ineffective.")]
    [ClangCheck(true)]
    public bool BugproneMisplacedWideningCast { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-move-forwarding-reference")]
    [Description("Warns if std::move is called on a forwarding reference, for example:")]
    [ClangCheck(true)]
    public bool BugproneMoveForwardingReference { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-multiple-statement-macro")]
    [Description("Detect multiple statement macros that are used in unbraced conditionals. Only the first statement of the macro will be inside the conditional and the other ones will be executed unconditionally.")]
    [ClangCheck(true)]
    public bool BugproneMultipleStatementMacro { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-parent-virtual-call")]
    [Description("Detects and fixes calls to grand-…parent virtual methods instead of calls to overridden parent’s virtual methods.")]
    [ClangCheck(true)]
    public bool BugproneParentVirtualCall { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-sizeof-container")]
    [Description("The check finds usages of sizeof on expressions of STL container types. Most likely the user wanted to use .size() instead.")]
    [ClangCheck(true)]
    public bool BugproneSizeofContainer { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-sizeof-expression")]
    [Description("The check finds usages of sizeof expressions which are most likely errors.")]
    [ClangCheck(true)]
    public bool BugproneSizeofExpression { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-string-constructor")]
    [Description("Finds string constructors that are suspicious and probably errors.")]
    [ClangCheck(true)]
    public bool BugproneStringConstructor { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-string-integer-assignment")]
    [Description("The check finds assignments of an integer to std::basic_string&lt;CharT&gt; (std::string, std::wstring, etc.). The source of the problem is the following assignment operator of std::basic_string&lt;CharT&gt;:")]
    [ClangCheck(true)]
    public bool BugproneStringIntegerAssignment { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-string-literal-with-embedded-nul")]
    [Description("Finds occurrences of string literal with embedded NUL character and validates their usage.")]
    [ClangCheck(true)]
    public bool BugproneStringLiteralWithEmbeddedNul { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-suspicious-enum-usage")]
    [Description("The checker detects various cases when an enum is probably misused (as a bitmask ).")]
    [ClangCheck(true)]
    public bool BugproneSuspiciousEnumUsage { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-suspicious-memset-usage")]
    [Description("This check finds memset() calls with potential mistakes in their arguments. Considering the function as void* memset(void* destination, int fill_value, size_t byte_count), the following cases are covered:")]
    [ClangCheck(true)]
    public bool BugproneSuspiciousMemsetUsage { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-suspicious-missing-comma")]
    [Description("String literals placed side-by-side are concatenated at translation phase 6 (after the preprocessor). This feature is used to represent long string literal on multiple lines.")]
    [ClangCheck(true)]
    public bool BugproneSuspiciousMissingComma { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-suspicious-semicolon")]
    [Description("Finds most instances of stray semicolons that unexpectedly alter the meaning of the code. More specifically, it looks for if, while, for and for-range statements whose body is a single semicolon, and then analyzes the context of the code (e.g. indentation) in an attempt to determine whether that is intentional.")]
    [ClangCheck(true)]
    public bool BugproneSuspiciousSemicolon { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-suspicious-string-compare")]
    [Description("Find suspicious usage of runtime string comparison functions. This check is valid in C and C++.")]
    [ClangCheck(true)]
    public bool BugproneSuspiciousStringCompare { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-swapped-arguments")]
    [Description("Finds potentially swapped arguments by looking at implicit conversions.")]
    [ClangCheck(true)]
    public bool BugproneSwappedArguments { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-terminating-continue")]
    [Description("Detects do while loops with a condition always evaluating to false that have a continue statement, as this continue terminates the loop effectively.")]
    [ClangCheck(true)]
    public bool BugproneTerminatingContinue { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-throw-keyword-missing")]
    [Description("Warns about a potentially missing throw keyword. If a temporary object is created, but the object’s type derives from (or is the same as) a class that has ‘EXCEPTION’, ‘Exception’ or ‘exception’ in its name, we can assume that the programmer’s intention was to throw that object.")]
    [ClangCheck(true)]
    public bool BugproneThrowKeywordMissing { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-too-small-loop-variable")]
    [Description("Detects those for loops that have a loop variable with a “too small” type which means this type can’t represent all values which are part of the iteration range.")]
    [ClangCheck(true)]
    public bool BugproneTooSmallLoopVariable { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-undefined-memory-manipulation")]
    [Description("Finds calls of memory manipulation functions memset(), memcpy() and memmove() on not TriviallyCopyable objects resulting in undefined behavior.")]
    [ClangCheck(true)]
    public bool BugproneUndefinedMemoryManipulation { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-undelegated-constructor")]
    [Description("Finds creation of temporary objects in constructors that look like a function call to another constructor of the same class.")]
    [ClangCheck(true)]
    public bool BugproneUndelegatedConstructor { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-unused-raii")]
    [Description("Finds temporaries that look like RAII objects.")]
    [ClangCheck(true)]
    public bool BugproneUnusedRaii { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-unused-return-value")]
    [Description("Warns on unused function return values. The checked funtions can be configured.")]
    [ClangCheck(true)]
    public bool BugproneUnusedReturnValue { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-use-after-move")]
    [Description("Warns if an object is used after it has been moved, for example:")]
    [ClangCheck(true)]
    public bool BugproneUseAfterMove { get; set; }


    [Category("Checks")]
    [DisplayName("bugprone-virtual-near-miss")]
    [Description("Warn if a function is a near miss (ie. the name is very similar and the function signiture is the same) to a virtual function from a base class.")]
    [ClangCheck(true)]
    public bool BugproneVirtualNearMiss { get; set; }


    [Category("Checks")]
    [DisplayName("cert-dcl03-c")]
    [Description("The cert-dcl03-c check is an alias, please see misc-static-assert for more information.")]
    [ClangCheck(true)]
    public bool CertDcl03C { get; set; }


    [Category("Checks")]
    [DisplayName("cert-dcl16-c")]
    [Description("The cert-dcl16-c check is an alias, please see readability-uppercase-literal-suffix for more information.")]
    [ClangCheck(true)]
    public bool CertDcl16C { get; set; }


    [Category("Checks")]
    [DisplayName("cert-dcl21-cpp")]
    [Description("This check flags postfix operator++ and operator-- declarations if the return type is not a const object. This also warns if the return type is a reference type.")]
    [ClangCheck(true)]
    public bool CertDcl21Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("cert-dcl50-cpp")]
    [Description("This check flags all function definitions (but not declarations) of C-style variadic functions.")]
    [ClangCheck(true)]
    public bool CertDcl50Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("cert-dcl54-cpp")]
    [Description("The cert-dcl54-cpp check is an alias, please see misc-new-delete-overloads for more information.")]
    [ClangCheck(true)]
    public bool CertDcl54Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("cert-dcl58-cpp")]
    [Description("Modification of the std or posix namespace can result in undefined behavior. This check warns for such modifications.")]
    [ClangCheck(true)]
    public bool CertDcl58Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("cert-dcl59-cpp")]
    [Description("The cert-dcl59-cpp check is an alias, please see google-build-namespaces for more information.")]
    [ClangCheck(true)]
    public bool CertDcl59Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("cert-env33-c")]
    [Description("This check flags calls to system(), popen(), and _popen(), which execute a command processor. It does not flag calls to system() with a null pointer argument, as such a call checks for the presence of a command processor but does not actually attempt to execute a command.")]
    [ClangCheck(true)]
    public bool CertEnv33C { get; set; }


    [Category("Checks")]
    [DisplayName("cert-err09-cpp")]
    [Description("The cert-err09-cpp check is an alias, please see misc-throw-by-value-catch-by-reference for more information.")]
    [ClangCheck(true)]
    public bool CertErr09Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("cert-err34-c")]
    [Description("This check flags calls to string-to-number conversion functions that do not verify the validity of the conversion, such as atoi() or scanf(). It does not flag calls to strtol(), or other, related conversion functions that do perform better error checking.")]
    [ClangCheck(true)]
    public bool CertErr34C { get; set; }


    [Category("Checks")]
    [DisplayName("cert-err52-cpp")]
    [Description("This check flags all call expressions involving setjmp() and longjmp().")]
    [ClangCheck(true)]
    public bool CertErr52Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("cert-err58-cpp")]
    [Description("This check flags all static or thread_local variable declarations where the initializer for the object may throw an exception.")]
    [ClangCheck(true)]
    public bool CertErr58Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("cert-err60-cpp")]
    [Description("This check flags all throw expressions where the exception object is not nothrow copy constructible.")]
    [ClangCheck(true)]
    public bool CertErr60Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("cert-err61-cpp")]
    [Description("The cert-err61-cpp check is an alias, please see misc-throw-by-value-catch-by-reference for more information.")]
    [ClangCheck(true)]
    public bool CertErr61Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("cert-fio38-c")]
    [Description("The cert-fio38-c check is an alias, please see misc-non-copyable-objects for more information.")]
    [ClangCheck(true)]
    public bool CertFio38C { get; set; }


    [Category("Checks")]
    [DisplayName("cert-flp30-c")]
    [Description("This check flags for loops where the induction expression has a floating-point type.")]
    [ClangCheck(true)]
    public bool CertFlp30C { get; set; }


    [Category("Checks")]
    [DisplayName("cert-msc30-c")]
    [Description("The cert-msc30-c check is an alias, please see cert-msc50-cpp for more information.")]
    [ClangCheck(true)]
    public bool CertMsc30C { get; set; }


    [Category("Checks")]
    [DisplayName("cert-msc32-c")]
    [Description("The cert-msc32-c check is an alias, please see cert-msc51-cpp for more information.")]
    [ClangCheck(true)]
    public bool CertMsc32C { get; set; }


    [Category("Checks")]
    [DisplayName("cert-msc50-cpp")]
    [Description("Pseudorandom number generators use mathematical algorithms to produce a sequence of numbers with good statistical properties, but the numbers produced are not genuinely random. The std::rand() function takes a seed (number), runs a mathematical operation on it and returns the result. By manipulating the seed the result can be predictable. This check warns for the usage of std::rand().")]
    [ClangCheck(true)]
    public bool CertMsc50Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("cert-msc51-cpp")]
    [Description("This check flags all pseudo-random number engines, engine adaptor instantiations and srand() when initialized or seeded with default argument, constant expression or any user-configurable type. Pseudo-random number engines seeded with a predictable value may cause vulnerabilities e.g. in security protocols. This is a CERT security rule, see MSC51-CPP. Ensure your random number generator is properly seeded and MSC32-C. Properly seed pseudorandom number generators.")]
    [ClangCheck(true)]
    public bool CertMsc51Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("cert-oop11-cpp")]
    [Description("The cert-oop11-cpp check is an alias, please see performance-move-constructor-init for more information.")]
    [ClangCheck(true)]
    public bool CertOop11Cpp { get; set; }


    [Category("Checks")]
    [DisplayName("core.CallAndMessage")]
    [Description("Check for logical errors for function calls and Objective-C message expressions (e.g., uninitialized arguments, null function pointers). ")]
    [ClangCheck(true)]
    public bool CoreCallAndMessage { get; set; }


    [Category("Checks")]
    [DisplayName("core.DivideZero")]
    [Description("Check for division by zero. ")]
    [ClangCheck(true)]
    public bool CoreDivideZero { get; set; }


    [Category("Checks")]
    [DisplayName("core.NonNullParamChecker")]
    [Description("Check for null pointers passed as arguments to a function whose arguments are marked with the nonnull attribute. ")]
    [ClangCheck(true)]
    public bool CoreNonNullParamChecker { get; set; }


    [Category("Checks")]
    [DisplayName("core.NullDereference")]
    [Description("Check for dereferences of null pointers. ")]
    [ClangCheck(true)]
    public bool CoreNullDereference { get; set; }


    [Category("Checks")]
    [DisplayName("core.StackAddressEscape")]
    [Description("Check that addresses of stack memory do not escape the function. ")]
    [ClangCheck(true)]
    public bool CoreStackAddressEscape { get; set; }


    [Category("Checks")]
    [DisplayName("core.UndefinedBinaryOperatorResult")]
    [Description("Check for undefined results of binary operators. ")]
    [ClangCheck(true)]
    public bool CoreUndefinedBinaryOperatorResult { get; set; }


    [Category("Checks")]
    [DisplayName("core.uninitialized.ArraySubscript")]
    [Description("Check for uninitialized values used as array subscripts. ")]
    [ClangCheck(true)]
    public bool CoreuninitializedArraySubscript { get; set; }


    [Category("Checks")]
    [DisplayName("core.uninitialized.Assign")]
    [Description("Check for assigning uninitialized values. ")]
    [ClangCheck(true)]
    public bool CoreuninitializedAssign { get; set; }


    [Category("Checks")]
    [DisplayName("core.uninitialized.Branch")]
    [Description("Check for uninitialized values used as branch conditions. ")]
    [ClangCheck(true)]
    public bool CoreuninitializedBranch { get; set; }


    [Category("Checks")]
    [DisplayName("core.uninitialized.CapturedBlockVariable")]
    [Description("Check for blocks that capture uninitialized values. ")]
    [ClangCheck(true)]
    public bool CoreuninitializedCapturedBlockVariable { get; set; }


    [Category("Checks")]
    [DisplayName("core.uninitialized.UndefReturn")]
    [Description("Check for uninitialized values being returned to the caller. ")]
    [ClangCheck(true)]
    public bool CoreuninitializedUndefReturn { get; set; }


    [Category("Checks")]
    [DisplayName("core.VLASize")]
    [Description("Check for declarations of VLA of undefined or zero size. ")]
    [ClangCheck(true)]
    public bool CoreVLASize { get; set; }


    [Category("Checks")]
    [DisplayName("cplusplus.NewDelete")]
    [Description("Check for double-free, use-after-free and offset problems involving C++  delete. ")]
    [ClangCheck(true)]
    public bool CplusplusNewDelete { get; set; }


    [Category("Checks")]
    [DisplayName("cplusplus.NewDeleteLeaks")]
    [Description("Check for memory leaks. Traces memory managed by new/ delete. ")]
    [ClangCheck(true)]
    public bool CplusplusNewDeleteLeaks { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-avoid-c-arrays")]
    [Description("The cppcoreguidelines-avoid-c-arrays check is an alias, please see modernize-avoid-c-arrays for more information.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesAvoidCArrays { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-avoid-goto")]
    [Description("The usage of goto for control flow is error prone and should be replaced with looping constructs. Only forward jumps in nested loops are accepted.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesAvoidGoto { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-avoid-magic-numbers")]
    [Description("The cppcoreguidelines-avoid-magic-numbers check is an alias, please see readability-magic-numbers for more information.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesAvoidMagicNumbers { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-c-copy-assignment-signature")]
    [Description("The cppcoreguidelines-c-copy-assignment-signature check is an alias, please see misc-unconventional-assign-operator for more information.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesCCopyAssignmentSignature { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-interfaces-global-init")]
    [Description("This check flags initializers of globals that access extern objects, and therefore can lead to order-of-initialization problems.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesInterfacesGlobalInit { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-macro-usage")]
    [Description("Finds macro usage that is considered problematic because better language constructs exist for the task.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesMacroUsage { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-narrowing-conversions")]
    [Description("Checks for silent narrowing conversions, e.g: int i = 0; i += 0.1;. While the issue is obvious in this former example, it might not be so in the following: void MyClass::f(double d) { int_member_ += d; }.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesNarrowingConversions { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-no-malloc")]
    [Description("This check handles C-Style memory management using malloc(), realloc(), calloc() and free(). It warns about its use and tries to suggest the use of an appropriate RAII object. Furthermore, it can be configured to check against a user-specified list of functions that are used for memory management (e.g. posix_memalign()). See C++ Core Guidelines.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesNoMalloc { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-non-private-member-variables-in-classes")]
    [Description("The cppcoreguidelines-non-private-member-variables-in-classes check is an alias, please see misc-non-private-member-variables-in-classes for more information.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesNonPrivateMemberVariablesInClasses { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-owning-memory")]
    [Description("This check implements the type-based semantics of gsl::owner&lt;T*&gt;, which allows static analysis on code, that uses raw pointers to handle resources like dynamic memory, but won’t introduce RAII concepts.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesOwningMemory { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-bounds-array-to-pointer-decay")]
    [Description("This check flags all array to pointer decays.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProBoundsArrayToPointerDecay { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-bounds-constant-array-index")]
    [Description("This check flags all array subscript expressions on static arrays and std::arrays that either do not have a constant integer expression index or are out of bounds (for std::array). For out-of-bounds checking of static arrays, see the -Warray-bounds Clang diagnostic.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProBoundsConstantArrayIndex { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-bounds-pointer-arithmetic")]
    [Description("This check flags all usage of pointer arithmetic, because it could lead to an invalid pointer. Subtraction of two pointers is not flagged by this check.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProBoundsPointerArithmetic { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-const-cast")]
    [Description("This check flags all uses of const_cast in C++ code.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeConstCast { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-cstyle-cast")]
    [Description("This check flags all use of C-style casts that perform a static_cast downcast, const_cast, or reinterpret_cast.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeCstyleCast { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-member-init")]
    [Description("The check flags user-defined constructor definitions that do not initialize all fields that would be left in an undefined state by default construction, e.g. builtins, pointers and record types without user-provided default constructors containing at least one such type. If these fields aren’t initialized, the constructor will leave some of the memory in an undefined state.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeMemberInit { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-reinterpret-cast")]
    [Description("This check flags all uses of reinterpret_cast in C++ code.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeReinterpretCast { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-static-cast-downcast")]
    [Description("This check flags all usages of static_cast, where a base class is casted to a derived class. In those cases, a fix-it is provided to convert the cast to a dynamic_cast.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeStaticCastDowncast { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-union-access")]
    [Description("This check flags all access to members of unions. Passing unions as a whole is not flagged.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeUnionAccess { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-vararg")]
    [Description("This check flags all calls to c-style vararg functions and all use of va_arg.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeVararg { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-slicing")]
    [Description("Flags slicing of member variables or vtable. Slicing happens when copying a derived object into a base object: the members of the derived object (both member variables and virtual member functions) will be discarded. This can be misleading especially for member function slicing, for example:")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesSlicing { get; set; }


    [Category("Checks")]
    [DisplayName("cppcoreguidelines-special-member-functions")]
    [Description("The check finds classes where some but not all of the special member functions are defined.")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesSpecialMemberFunctions { get; set; }


    [Category("Checks")]
    [DisplayName("deadcode.DeadStores")]
    [Description("Check for values stored to variables that are never read afterwards. ")]
    [ClangCheck(true)]
    public bool DeadcodeDeadStores { get; set; }


    [Category("Checks")]
    [DisplayName("fuchsia-default-arguments")]
    [Description("Warns if a function or method is declared or called with default arguments.")]
    [ClangCheck(true)]
    public bool FuchsiaDefaultArguments { get; set; }


    [Category("Checks")]
    [DisplayName("fuchsia-header-anon-namespaces")]
    [Description("The fuchsia-header-anon-namespaces check is an alias, please see google-build-namespace for more information.")]
    [ClangCheck(true)]
    public bool FuchsiaHeaderAnonNamespaces { get; set; }


    [Category("Checks")]
    [DisplayName("fuchsia-multiple-inheritance")]
    [Description("Warns if a class inherits from multiple classes that are not pure virtual.")]
    [ClangCheck(true)]
    public bool FuchsiaMultipleInheritance { get; set; }


    [Category("Checks")]
    [DisplayName("fuchsia-overloaded-operator")]
    [Description("Warns if an operator is overloaded, except for the assignment (copy and move) operators.")]
    [ClangCheck(true)]
    public bool FuchsiaOverloadedOperator { get; set; }


    [Category("Checks")]
    [DisplayName("fuchsia-restrict-system-includes")]
    [Description("Checks for allowed system includes and suggests removal of any others.")]
    [ClangCheck(true)]
    public bool FuchsiaRestrictSystemIncludes { get; set; }


    [Category("Checks")]
    [DisplayName("fuchsia-statically-constructed-objects")]
    [Description("Warns if global, non-trivial objects with static storage are constructed, unless the object is statically initialized with a constexpr constructor or has no explicit constructor.")]
    [ClangCheck(true)]
    public bool FuchsiaStaticallyConstructedObjects { get; set; }


    [Category("Checks")]
    [DisplayName("fuchsia-trailing-return")]
    [Description("Functions that have trailing returns are disallowed, except for those using decltype specifiers and lambda with otherwise unutterable return types.")]
    [ClangCheck(true)]
    public bool FuchsiaTrailingReturn { get; set; }


    [Category("Checks")]
    [DisplayName("fuchsia-virtual-inheritance")]
    [Description("Warns if classes are defined with virtual inheritance.")]
    [ClangCheck(true)]
    public bool FuchsiaVirtualInheritance { get; set; }


    [Category("Checks")]
    [DisplayName("google-build-explicit-make-pair")]
    [Description("Check that make_pair’s template arguments are deduced.")]
    [ClangCheck(true)]
    public bool GoogleBuildExplicitMakePair { get; set; }


    [Category("Checks")]
    [DisplayName("google-build-namespaces")]
    [Description("cert-dcl59-cpp redirects here as an alias for this check. fuchsia-header-anon-namespaces redirects here as an alias for this check.")]
    [ClangCheck(true)]
    public bool GoogleBuildNamespaces { get; set; }


    [Category("Checks")]
    [DisplayName("google-build-using-namespace")]
    [Description("Finds using namespace directives.")]
    [ClangCheck(true)]
    public bool GoogleBuildUsingNamespace { get; set; }


    [Category("Checks")]
    [DisplayName("google-default-arguments")]
    [Description("Checks that default arguments are not given for virtual methods.")]
    [ClangCheck(true)]
    public bool GoogleDefaultArguments { get; set; }


    [Category("Checks")]
    [DisplayName("google-explicit-constructor")]
    [Description("Checks that constructors callable with a single argument and conversion operators are marked explicit to avoid the risk of unintentional implicit conversions.")]
    [ClangCheck(true)]
    public bool GoogleExplicitConstructor { get; set; }


    [Category("Checks")]
    [DisplayName("google-global-names-in-headers")]
    [Description("Flag global namespace pollution in header files. Right now it only triggers on using declarations and directives.")]
    [ClangCheck(true)]
    public bool GoogleGlobalNamesInHeaders { get; set; }


    [Category("Checks")]
    [DisplayName("google-objc-avoid-throwing-exception")]
    [Description("Finds uses of throwing exceptions usages in Objective-C files.")]
    [ClangCheck(true)]
    public bool GoogleObjcAvoidThrowingException { get; set; }


    [Category("Checks")]
    [DisplayName("google-objc-function-naming")]
    [Description("Finds function declarations in Objective-C files that do not follow the pattern described in the Google Objective-C Style Guide.")]
    [ClangCheck(true)]
    public bool GoogleObjcFunctionNaming { get; set; }


    [Category("Checks")]
    [DisplayName("google-objc-global-variable-declaration")]
    [Description("Finds global variable declarations in Objective-C files that do not follow the pattern of variable names in Google’s Objective-C Style Guide.")]
    [ClangCheck(true)]
    public bool GoogleObjcGlobalVariableDeclaration { get; set; }


    [Category("Checks")]
    [DisplayName("google-readability-braces-around-statements")]
    [Description("The google-readability-braces-around-statements check is an alias, please see readability-braces-around-statements for more information.")]
    [ClangCheck(true)]
    public bool GoogleReadabilityBracesAroundStatements { get; set; } = true;


    [Category("Checks")]
    [DisplayName("google-readability-casting")]
    [Description("Finds usages of C-style casts.")]
    [ClangCheck(true)]
    public bool GoogleReadabilityCasting { get; set; } = true;


    [Category("Checks")]
    [DisplayName("google-readability-function-size")]
    [Description("The google-readability-function-size check is an alias, please see readability-function-size for more information.")]
    [ClangCheck(true)]
    public bool GoogleReadabilityFunctionSize { get; set; } = true;


    [Category("Checks")]
    [DisplayName("google-readability-namespace-comments")]
    [Description("The google-readability-namespace-comments check is an alias, please see llvm-namespace-comment for more information.")]
    [ClangCheck(true)]
    public bool GoogleReadabilityNamespaceComments { get; set; } = true;


    [Category("Checks")]
    [DisplayName("google-readability-todo")]
    [Description("Finds TODO comments without a username or bug number.")]
    [ClangCheck(true)]
    public bool GoogleReadabilityTodo { get; set; } = true;


    [Category("Checks")]
    [DisplayName("google-runtime-int")]
    [Description("Finds uses of short, long and long long and suggest replacing them with u?intXX(_t)?.")]
    [ClangCheck(true)]
    public bool GoogleRuntimeInt { get; set; }


    [Category("Checks")]
    [DisplayName("google-runtime-operator")]
    [Description("Finds overloads of unary operator &amp;.")]
    [ClangCheck(true)]
    public bool GoogleRuntimeOperator { get; set; }


    [Category("Checks")]
    [DisplayName("google-runtime-references")]
    [Description("Checks the usage of non-constant references in function parameters.")]
    [ClangCheck(true)]
    public bool GoogleRuntimeReferences { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-avoid-c-arrays")]
    [Description("The hicpp-avoid-c-arrays check is an alias, please see modernize-avoid-c-arrays for more information.")]
    [ClangCheck(true)]
    public bool HicppAvoidCArrays { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-avoid-goto")]
    [Description("The hicpp-avoid-goto check is an alias to cppcoreguidelines-avoid-goto. Rule 6.3.1 High Integrity C++ requires that goto only skips parts of a block and is not used for other reasons.")]
    [ClangCheck(true)]
    public bool HicppAvoidGoto { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-braces-around-statements")]
    [Description("The hicpp-braces-around-statements check is an alias, please see readability-braces-around-statements for more information. It enforces the rule 6.1.1.")]
    [ClangCheck(true)]
    public bool HicppBracesAroundStatements { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-deprecated-headers")]
    [Description("The hicpp-deprecated-headers check is an alias, please see modernize-deprecated-headers for more information. It enforces the rule 1.3.3.")]
    [ClangCheck(true)]
    public bool HicppDeprecatedHeaders { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-exception-baseclass")]
    [Description("Ensure that every value that in a throw expression is an instance of std::exception.")]
    [ClangCheck(true)]
    public bool HicppExceptionBaseclass { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-explicit-conversions")]
    [Description("This check is an alias for google-explicit-constructor. Used to enforce parts of rule 5.4.1. This check will enforce that constructors and conversion operators are marked explicit. Other forms of casting checks are implemented in other places. The following checks can be used to check for more forms of casting:")]
    [ClangCheck(true)]
    public bool HicppExplicitConversions { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-function-size")]
    [Description("This check is an alias for readability-function-size. Useful to enforce multiple sections on function complexity.")]
    [ClangCheck(true)]
    public bool HicppFunctionSize { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-invalid-access-moved")]
    [Description("This check is an alias for bugprone-use-after-move.")]
    [ClangCheck(true)]
    public bool HicppInvalidAccessMoved { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-member-init")]
    [Description("This check is an alias for cppcoreguidelines-pro-type-member-init. Implements the check for rule 12.4.2 to initialize class members in the right order.")]
    [ClangCheck(true)]
    public bool HicppMemberInit { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-move-const-arg")]
    [Description("The hicpp-move-const-arg check is an alias, please see performance-move-const-arg for more information. It enforces the rule 17.3.1.")]
    [ClangCheck(true)]
    public bool HicppMoveConstArg { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-multiway-paths-covered")]
    [Description("This check discovers situations where code paths are not fully-covered. It furthermore suggests using if instead of switch if the code will be more clear. The rule 6.1.2 and rule 6.1.4 of the High Integrity C++ Coding Standard are enforced.")]
    [ClangCheck(true)]
    public bool HicppMultiwayPathsCovered { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-named-parameter")]
    [Description("This check is an alias for readability-named-parameter.")]
    [ClangCheck(true)]
    public bool HicppNamedParameter { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-new-delete-operators")]
    [Description("This check is an alias for misc-new-delete-overloads. Implements rule 12.3.1 to ensure the new and delete operators have the correct signature.")]
    [ClangCheck(true)]
    public bool HicppNewDeleteOperators { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-no-array-decay")]
    [Description("The hicpp-no-array-decay check is an alias, please see cppcoreguidelines-pro-bounds-array-to-pointer-decay for more information. It enforces the rule 4.1.1.")]
    [ClangCheck(true)]
    public bool HicppNoArrayDecay { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-no-assembler")]
    [Description("Check for assembler statements. No fix is offered.")]
    [ClangCheck(true)]
    public bool HicppNoAssembler { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-noexcept-move")]
    [Description("This check is an alias for misc-noexcept-moveconstructor. Checks rule 12.5.4 to mark move assignment and move construction noexcept.")]
    [ClangCheck(true)]
    public bool HicppNoexceptMove { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-no-malloc")]
    [Description("The hicpp-no-malloc check is an alias, please see cppcoreguidelines-no-malloc for more information. It enforces the rule 5.3.2.")]
    [ClangCheck(true)]
    public bool HicppNoMalloc { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-signed-bitwise")]
    [Description("Finds uses of bitwise operations on signed integer types, which may lead to undefined or implementation defined behaviour.")]
    [ClangCheck(true)]
    public bool HicppSignedBitwise { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-special-member-functions")]
    [Description("This check is an alias for cppcoreguidelines-special-member-functions. Checks that special member functions have the correct signature, according to rule 12.5.7.")]
    [ClangCheck(true)]
    public bool HicppSpecialMemberFunctions { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-static-assert")]
    [Description("The hicpp-static-assert check is an alias, please see misc-static-assert for more information. It enforces the rule 7.1.10.")]
    [ClangCheck(true)]
    public bool HicppStaticAssert { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-undelegated-constructor")]
    [Description("This check is an alias for bugprone-undelegated-constructor. Partially implements rule 12.4.5 to find misplaced constructor calls inside a constructor.")]
    [ClangCheck(true)]
    public bool HicppUndelegatedConstructor { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-uppercase-literal-suffix")]
    [Description("The hicpp-uppercase-literal-suffix check is an alias, please see readability-uppercase-literal-suffix for more information.")]
    [ClangCheck(true)]
    public bool HicppUppercaseLiteralSuffix { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-use-auto")]
    [Description("The hicpp-use-auto check is an alias, please see modernize-use-auto for more information. It enforces the rule 7.1.8.")]
    [ClangCheck(true)]
    public bool HicppUseAuto { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-use-emplace")]
    [Description("The hicpp-use-emplace check is an alias, please see modernize-use-emplace for more information. It enforces the rule 17.4.2.")]
    [ClangCheck(true)]
    public bool HicppUseEmplace { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-use-equals-default")]
    [Description("This check is an alias for modernize-use-equals-default. Implements rule 12.5.1 to explicitly default special member functions.")]
    [ClangCheck(true)]
    public bool HicppUseEqualsDefault { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-use-equals-delete")]
    [Description("This check is an alias for modernize-use-equals-delete. Implements rule 12.5.1 to explicitly default or delete special member functions.")]
    [ClangCheck(true)]
    public bool HicppUseEqualsDelete { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-use-noexcept")]
    [Description("The hicpp-use-noexcept check is an alias, please see modernize-use-noexcept for more information. It enforces the rule 1.3.5.")]
    [ClangCheck(true)]
    public bool HicppUseNoexcept { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-use-nullptr")]
    [Description("The hicpp-use-nullptr check is an alias, please see modernize-use-nullptr for more information. It enforces the rule 2.5.3.")]
    [ClangCheck(true)]
    public bool HicppUseNullptr { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-use-override")]
    [Description("This check is an alias for modernize-use-override. Implements rule 10.2.1 to declare a virtual function override when overriding.")]
    [ClangCheck(true)]
    public bool HicppUseOverride { get; set; }


    [Category("Checks")]
    [DisplayName("hicpp-vararg")]
    [Description("The hicpp-vararg check is an alias, please see cppcoreguidelines-pro-type-vararg for more information. It enforces the rule 14.1.1.")]
    [ClangCheck(true)]
    public bool HicppVararg { get; set; }


    [Category("Checks")]
    [DisplayName("llvm-header-guard")]
    [Description("Finds and fixes header guards that do not adhere to LLVM style.")]
    [ClangCheck(true)]
    public bool LlvmHeaderGuard { get; set; }


    [Category("Checks")]
    [DisplayName("llvm-include-order")]
    [Description("Checks the correct order of #includes.")]
    [ClangCheck(true)]
    public bool LlvmIncludeOrder { get; set; }


    [Category("Checks")]
    [DisplayName("llvm-namespace-comment")]
    [Description("google-readability-namespace-comments redirects here as an alias for this check.")]
    [ClangCheck(true)]
    public bool LlvmNamespaceComment { get; set; }


    [Category("Checks")]
    [DisplayName("llvm-twine-local")]
    [Description("Looks for local Twine variables which are prone to use after frees and should be generally avoided.")]
    [ClangCheck(true)]
    public bool LlvmTwineLocal { get; set; }


    [Category("Checks")]
    [DisplayName("misc-definitions-in-headers")]
    [Description("Finds non-extern non-inline function and variable definitions in header files, which can lead to potential ODR violations in case these headers are included from multiple translation units.")]
    [ClangCheck(true)]
    public bool MiscDefinitionsInHeaders { get; set; }


    [Category("Checks")]
    [DisplayName("misc-misplaced-const")]
    [Description("This check diagnoses when a const qualifier is applied to a typedef to a pointer type rather than to the pointee, because such constructs are often misleading to developers because the const applies to the pointer rather than the pointee.")]
    [ClangCheck(true)]
    public bool MiscMisplacedConst { get; set; }


    [Category("Checks")]
    [DisplayName("misc-new-delete-overloads")]
    [Description("cert-dcl54-cpp redirects here as an alias for this check.")]
    [ClangCheck(true)]
    public bool MiscNewDeleteOverloads { get; set; }


    [Category("Checks")]
    [DisplayName("misc-non-copyable-objects")]
    [Description("cert-fio38-c redirects here as an alias for this check.")]
    [ClangCheck(true)]
    public bool MiscNonCopyableObjects { get; set; }


    [Category("Checks")]
    [DisplayName("misc-non-private-member-variables-in-classes")]
    [Description("cppcoreguidelines-non-private-member-variables-in-classes redirects here as an alias for this check.")]
    [ClangCheck(true)]
    public bool MiscNonPrivateMemberVariablesInClasses { get; set; }


    [Category("Checks")]
    [DisplayName("misc-redundant-expression")]
    [Description("Detect redundant expressions which are typically errors due to copy-paste.")]
    [ClangCheck(true)]
    public bool MiscRedundantExpression { get; set; }


    [Category("Checks")]
    [DisplayName("misc-static-assert")]
    [Description("cert-dcl03-c redirects here as an alias for this check.")]
    [ClangCheck(true)]
    public bool MiscStaticAssert { get; set; }


    [Category("Checks")]
    [DisplayName("misc-throw-by-value-catch-by-reference")]
    [Description("“cert-err09-cpp” redirects here as an alias for this check. “cert-err61-cpp” redirects here as an alias for this check.")]
    [ClangCheck(true)]
    public bool MiscThrowByValueCatchByReference { get; set; }


    [Category("Checks")]
    [DisplayName("misc-unconventional-assign-operator")]
    [Description("Finds declarations of assign operators with the wrong return and/or argument types and definitions with good return type but wrong return statements.")]
    [ClangCheck(true)]
    public bool MiscUnconventionalAssignOperator { get; set; }


    [Category("Checks")]
    [DisplayName("misc-uniqueptr-reset-release")]
    [Description("Find and replace unique_ptr::reset(release()) with std::move().")]
    [ClangCheck(true)]
    public bool MiscUniqueptrResetRelease { get; set; }


    [Category("Checks")]
    [DisplayName("misc-unused-alias-decls")]
    [Description("Finds unused namespace alias declarations.")]
    [ClangCheck(true)]
    public bool MiscUnusedAliasDecls { get; set; }


    [Category("Checks")]
    [DisplayName("misc-unused-parameters")]
    [Description("Finds unused function parameters. Unused parameters may signify a bug in the code (e.g. when a different parameter is used instead). The suggested fixes either comment parameter name out or remove the parameter completely, if all callers of the function are in the same translation unit and can be updated.")]
    [ClangCheck(true)]
    public bool MiscUnusedParameters { get; set; }


    [Category("Checks")]
    [DisplayName("misc-unused-using-decls")]
    [Description("Finds unused using declarations.")]
    [ClangCheck(true)]
    public bool MiscUnusedUsingDecls { get; set; }


    [Category("Checks")]
    [DisplayName("modernize-avoid-bind")]
    [Description("The check finds uses of std::bind and replaces simple uses with lambdas. Lambdas will use value-capture where required.")]
    [ClangCheck(true)]
    public bool ModernizeAvoidBind { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-avoid-c-arrays")]
    [Description("cppcoreguidelines-avoid-c-arrays redirects here as an alias for this check.")]
    [ClangCheck(true)]
    public bool ModernizeAvoidCArrays { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-concat-nested-namespaces")]
    [Description("Checks for use of nested namespaces such as namespace a { namespace b { ... } } and suggests changing to the more concise syntax introduced in C++17: namespace a::b { ... }. Inline namespaces are not modified.")]
    [ClangCheck(true)]
    public bool ModernizeConcatNestedNamespaces { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-deprecated-headers")]
    [Description("Some headers from C library were deprecated in C++ and are no longer welcome in C++ codebases. Some have no effect in C++. For more details refer to the C++ 14 Standard [depr.c.headers] section.")]
    [ClangCheck(true)]
    public bool ModernizeDeprecatedHeaders { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-deprecated-ios-base-aliases")]
    [Description("Detects usage of the deprecated member types of std::ios_base and replaces those that have a non-deprecated equivalent.")]
    [ClangCheck(true)]
    public bool ModernizeDeprecatedIosBaseAliases { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-loop-convert")]
    [Description("This check converts for(...; ...; ...) loops to use the new range-based loops in C++11.")]
    [ClangCheck(true)]
    public bool ModernizeLoopConvert { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-make-shared")]
    [Description("This check finds the creation of std::shared_ptr objects by explicitly calling the constructor and a new expression, and replaces it with a call to std::make_shared.")]
    [ClangCheck(true)]
    public bool ModernizeMakeShared { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-make-unique")]
    [Description("This check finds the creation of std::unique_ptr objects by explicitly calling the constructor and a new expression, and replaces it with a call to std::make_unique, introduced in C++14.")]
    [ClangCheck(true)]
    public bool ModernizeMakeUnique { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-pass-by-value")]
    [Description("With move semantics added to the language and the standard library updated with move constructors added for many types it is now interesting to take an argument directly by value, instead of by const-reference, and then copy. This check allows the compiler to take care of choosing the best way to construct the copy.")]
    [ClangCheck(true)]
    public bool ModernizePassByValue { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-raw-string-literal")]
    [Description("This check selectively replaces string literals containing escaped characters with raw string literals.")]
    [ClangCheck(true)]
    public bool ModernizeRawStringLiteral { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-redundant-void-arg")]
    [Description("Find and remove redundant void argument lists.")]
    [ClangCheck(true)]
    public bool ModernizeRedundantVoidArg { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-replace-auto-ptr")]
    [Description("This check replaces the uses of the deprecated class std::auto_ptr by std::unique_ptr (introduced in C++11). The transfer of ownership, done by the copy-constructor and the assignment operator, is changed to match std::unique_ptr usage by using explicit calls to std::move().")]
    [ClangCheck(true)]
    public bool ModernizeReplaceAutoPtr { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-replace-random-shuffle")]
    [Description("This check will find occurrences of std::random_shuffle and replace it with std::shuffle. In C++17 std::random_shuffle will no longer be available and thus we need to replace it.")]
    [ClangCheck(true)]
    public bool ModernizeReplaceRandomShuffle { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-return-braced-init-list")]
    [Description("Replaces explicit calls to the constructor in a return with a braced initializer list. This way the return type is not needlessly duplicated in the function definition and the return statement.")]
    [ClangCheck(true)]
    public bool ModernizeReturnBracedInitList { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-shrink-to-fit")]
    [Description("Replace copy and swap tricks on shrinkable containers with the shrink_to_fit() method call.")]
    [ClangCheck(true)]
    public bool ModernizeShrinkToFit { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-unary-static-assert")]
    [Description("The check diagnoses any static_assert declaration with an empty string literal and provides a fix-it to replace the declaration with a single-argument static_assert declaration.")]
    [ClangCheck(true)]
    public bool ModernizeUnaryStaticAssert { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-use-auto")]
    [Description("This check is responsible for using the auto type specifier for variable declarations to improve code readability and maintainability. For example:")]
    [ClangCheck(true)]
    public bool ModernizeUseAuto { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-use-bool-literals")]
    [Description("Finds integer literals which are cast to bool.")]
    [ClangCheck(true)]
    public bool ModernizeUseBoolLiterals { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-use-default-member-init")]
    [Description("This check converts a default constructor’s member initializers into the new default member initializers in C++11. Other member initializers that match the default member initializer are removed. This can reduce repeated code or allow use of ‘= default’.")]
    [ClangCheck(true)]
    public bool ModernizeUseDefaultMemberInit { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-use-emplace")]
    [Description("The check flags insertions to an STL-style container done by calling the push_back method with an explicitly-constructed temporary of the container element type. In this case, the corresponding emplace_back method results in less verbose and potentially more efficient code. Right now the check doesn’t support push_front and insert. It also doesn’t support insert functions for associative containers because replacing insert with emplace may result in speed regression, but it might get support with some addition flag in the future.")]
    [ClangCheck(true)]
    public bool ModernizeUseEmplace { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-use-equals-default")]
    [Description("This check replaces default bodies of special member functions with = default;. The explicitly defaulted function declarations enable more opportunities in optimization, because the compiler might treat explicitly defaulted functions as trivial.")]
    [ClangCheck(true)]
    public bool ModernizeUseEqualsDefault { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-use-equals-delete")]
    [Description("This check marks unimplemented private special member functions with = delete. To avoid false-positives, this check only applies in a translation unit that has all other member functions implemented.")]
    [ClangCheck(true)]
    public bool ModernizeUseEqualsDelete { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-use-noexcept")]
    [Description("This check replaces deprecated dynamic exception specifications with the appropriate noexcept specification (introduced in C++11).  By default this check will replace throw() with noexcept, and throw(&lt;exception&gt;[,...]) or throw(...) with noexcept(false).")]
    [ClangCheck(true)]
    public bool ModernizeUseNoexcept { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-use-nullptr")]
    [Description("The check converts the usage of null pointer constants (eg. NULL, 0) to use the new C++11 nullptr keyword.")]
    [ClangCheck(true)]
    public bool ModernizeUseNullptr { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-use-override")]
    [Description("Use C++11’s override and remove virtual where applicable.")]
    [ClangCheck(true)]
    public bool ModernizeUseOverride { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-use-transparent-functors")]
    [Description("Prefer transparent functors to non-transparent ones. When using transparent functors, the type does not need to be repeated. The code is easier to read, maintain and less prone to errors. It is not possible to introduce unwanted conversions.")]
    [ClangCheck(true)]
    public bool ModernizeUseTransparentFunctors { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-use-uncaught-exceptions")]
    [Description("This check will warn on calls to std::uncaught_exception and replace them with calls to std::uncaught_exceptions, since std::uncaught_exception was deprecated in C++17.")]
    [ClangCheck(true)]
    public bool ModernizeUseUncaughtExceptions { get; set; } = true;


    [Category("Checks")]
    [DisplayName("modernize-use-using")]
    [Description("The check converts the usage of typedef with using keyword.")]
    [ClangCheck(true)]
    public bool ModernizeUseUsing { get; set; } = true;


    [Category("Checks")]
    [DisplayName("mpi-buffer-deref")]
    [Description("This check verifies if a buffer passed to an MPI (Message Passing Interface) function is sufficiently dereferenced. Buffers should be passed as a single pointer or array. As MPI function signatures specify void * for their buffer types, insufficiently dereferenced buffers can be passed, like for example as double pointers or multidimensional arrays, without a compiler warning emitted.")]
    [ClangCheck(true)]
    public bool MpiBufferDeref { get; set; }


    [Category("Checks")]
    [DisplayName("mpi-type-mismatch")]
    [Description("This check verifies if buffer type and MPI (Message Passing Interface) datatype pairs match for used MPI functions. All MPI datatypes defined by the MPI standard (3.1) are verified by this check. User defined typedefs, custom MPI datatypes and null pointer constants are skipped, in the course of verification.")]
    [ClangCheck(true)]
    public bool MpiTypeMismatch { get; set; }


    [Category("Checks")]
    [DisplayName("nullability.NullableDereferenced")]
    [Description("Warns when a nullable pointer is dereferenced. ")]
    [ClangCheck(true)]
    public bool NullabilityNullableDereferenced { get; set; }


    [Category("Checks")]
    [DisplayName("nullability.NullablePassedToNonnull")]
    [Description("Warns when a nullable pointer is passed to a pointer which has a _Nonnull type. ")]
    [ClangCheck(true)]
    public bool NullabilityNullablePassedToNonnull { get; set; }


    [Category("Checks")]
    [DisplayName("nullability.NullPassedToNonnull")]
    [Description("Warns when a null pointer is passed to a pointer which has a  _Nonnull type. ")]
    [ClangCheck(true)]
    public bool NullabilityNullPassedToNonnull { get; set; }


    [Category("Checks")]
    [DisplayName("nullability.NullReturnedFromNonnull")]
    [Description("Warns when a null pointer is returned from a function that has  _Nonnull return type. ")]
    [ClangCheck(true)]
    public bool NullabilityNullReturnedFromNonnull { get; set; }


    [Category("Checks")]
    [DisplayName("objc-avoid-nserror-init")]
    [Description("Finds improper initialization of NSError objects.")]
    [ClangCheck(true)]
    public bool ObjcAvoidNserrorInit { get; set; }


    [Category("Checks")]
    [DisplayName("objc-avoid-spinlock")]
    [Description("Finds usages of OSSpinlock, which is deprecated due to potential livelock problems.")]
    [ClangCheck(true)]
    public bool ObjcAvoidSpinlock { get; set; }


    [Category("Checks")]
    [DisplayName("objc-forbidden-subclassing")]
    [Description("Finds Objective-C classes which are subclasses of classes which are not designed to be subclassed.")]
    [ClangCheck(true)]
    public bool ObjcForbiddenSubclassing { get; set; }


    [Category("Checks")]
    [DisplayName("objc-property-declaration")]
    [Description("Finds property declarations in Objective-C files that do not follow the pattern of property names in Apple’s programming guide. The property name should be in the format of Lower Camel Case.")]
    [ClangCheck(true)]
    public bool ObjcPropertyDeclaration { get; set; }


    [Category("Checks")]
    [DisplayName("optin.cplusplus.VirtualCall")]
    [Description("Check virtual member function calls during construction or  destruction. ")]
    [ClangCheck(true)]
    public bool OptincplusplusVirtualCall { get; set; }


    [Category("Checks")]
    [DisplayName("optin.mpi.MPI-Checker")]
    [Description("Checks MPI code ")]
    [ClangCheck(true)]
    public bool OptinmpiMPIChecker { get; set; }


    [Category("Checks")]
    [DisplayName("optin.osx.cocoa.localizability.EmptyLocalizationContextChecker")]
    [Description("Check that NSLocalizedString macros include a comment for context. ")]
    [ClangCheck(true)]
    public bool OptinosxcocoalocalizabilityEmptyLocalizationContextChecker { get; set; }


    [Category("Checks")]
    [DisplayName("optin.osx.cocoa.localizability.NonLocalizedStringChecker")]
    [Description("Warns about uses of non-localized NSStrings passed to UI methods  expecting localized NSStrings ")]
    [ClangCheck(true)]
    public bool OptinosxcocoalocalizabilityNonLocalizedStringChecker { get; set; }


    [Category("Checks")]
    [DisplayName("osx.API")]
    [Description("Check for proper uses of various Apple APIs: dispatch_once ")]
    [ClangCheck(true)]
    public bool OsxAPI { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.AtSync")]
    [Description("Check for nil pointers used as mutexes for @synchronized. ")]
    [ClangCheck(true)]
    public bool OsxcocoaAtSync { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.ClassRelease")]
    [Description("Check for sending retain, release, or  autorelease directly to a class. ")]
    [ClangCheck(true)]
    public bool OsxcocoaClassRelease { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.Dealloc")]
    [Description("Warn about Objective-C classes that lack a correct implementation  of -dealloc. ")]
    [ClangCheck(true)]
    public bool OsxcocoaDealloc { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.IncompatibleMethodTypes")]
    [Description("Check for an incompatible type signature when overriding an Objective-C method. ")]
    [ClangCheck(true)]
    public bool OsxcocoaIncompatibleMethodTypes { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.NilArg")]
    [Description("Check for prohibited nil arguments in specific Objective-C method calls: - caseInsensitiveCompare: - compare: - compare:options: - compare:options:range: - compare:options:range:locale: - componentsSeparatedByCharactersInSet: - initWithFormat: ")]
    [ClangCheck(true)]
    public bool OsxcocoaNilArg { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.NSAutoreleasePool")]
    [Description("Warn for suboptimal uses of NSAutoreleasePool in Objective-C GC mode (-fobjc-gc compiler option). ")]
    [ClangCheck(true)]
    public bool OsxcocoaNSAutoreleasePool { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.NSError")]
    [Description("Check usage of NSError** parameters. ")]
    [ClangCheck(true)]
    public bool OsxcocoaNSError { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.ObjCGenerics")]
    [Description("Check for type errors when using Objective-C generics ")]
    [ClangCheck(true)]
    public bool OsxcocoaObjCGenerics { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.RetainCount")]
    [Description("Check for leaks and violations of the Cocoa Memory Management rules. ")]
    [ClangCheck(true)]
    public bool OsxcocoaRetainCount { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.SelfInit")]
    [Description("Check that self is properly initialized inside an initializer method. ")]
    [ClangCheck(true)]
    public bool OsxcocoaSelfInit { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.SuperDealloc")]
    [Description("Warn about improper use of '[super dealloc]' in Objective-C ")]
    [ClangCheck(true)]
    public bool OsxcocoaSuperDealloc { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.UnusedIvars")]
    [Description("Warn about private ivars that are never used. ")]
    [ClangCheck(true)]
    public bool OsxcocoaUnusedIvars { get; set; }


    [Category("Checks")]
    [DisplayName("osx.cocoa.VariadicMethodTypes")]
    [Description("Check for passing non-Objective-C types to variadic collection initialization methods that expect only Objective-C types. ")]
    [ClangCheck(true)]
    public bool OsxcocoaVariadicMethodTypes { get; set; }


    [Category("Checks")]
    [DisplayName("osx.coreFoundation.CFError")]
    [Description("Check usage of CFErrorRef* parameters. ")]
    [ClangCheck(true)]
    public bool OsxcoreFoundationCFError { get; set; }


    [Category("Checks")]
    [DisplayName("osx.coreFoundation.CFNumber")]
    [Description("Check for improper uses of CFNumberCreate. ")]
    [ClangCheck(true)]
    public bool OsxcoreFoundationCFNumber { get; set; }


    [Category("Checks")]
    [DisplayName("osx.coreFoundation.CFRetainRelease")]
    [Description("Check for null arguments to CFRetain, CFRelease, CFMakeCollectable. ")]
    [ClangCheck(true)]
    public bool OsxcoreFoundationCFRetainRelease { get; set; }


    [Category("Checks")]
    [DisplayName("osx.coreFoundation.containers.OutOfBounds")]
    [Description("Checks for index out-of-bounds when using CFArray API. ")]
    [ClangCheck(true)]
    public bool OsxcoreFoundationcontainersOutOfBounds { get; set; }


    [Category("Checks")]
    [DisplayName("osx.coreFoundation.containers.PointerSizedValues")]
    [Description("Warns if CFArray, CFDictionary, CFSet are created with non-pointer-size values. ")]
    [ClangCheck(true)]
    public bool OsxcoreFoundationcontainersPointerSizedValues { get; set; }


    [Category("Checks")]
    [DisplayName("osx.NumberObjectConversion")]
    [Description("Check for erroneous conversions of objects representing numbers  into numbers ")]
    [ClangCheck(true)]
    public bool OsxNumberObjectConversion { get; set; }


    [Category("Checks")]
    [DisplayName("osx.SecKeychainAPI")]
    [Description("Check for improper uses of the Security framework's Keychain APIs: SecKeychainItemCopyContent SecKeychainFindGenericPassword SecKeychainFindInternetPassword SecKeychainItemFreeContent SecKeychainItemCopyAttributesAndData SecKeychainItemFreeAttributesAndData ")]
    [ClangCheck(true)]
    public bool OsxSecKeychainAPI { get; set; }


    [Category("Checks")]
    [DisplayName("performance-faster-string-find")]
    [Description("Optimize calls to std::string::find() and friends when the needle passed is a single character string literal. The character literal overload is more efficient.")]
    [ClangCheck(true)]
    public bool PerformanceFasterStringFind { get; set; }


    [Category("Checks")]
    [DisplayName("performance-for-range-copy")]
    [Description("Finds C++11 for ranges where the loop variable is copied in each iteration but it would suffice to obtain it by const reference.")]
    [ClangCheck(true)]
    public bool PerformanceForRangeCopy { get; set; }


    [Category("Checks")]
    [DisplayName("performance-implicit-conversion-in-loop")]
    [Description("This warning appears in a range-based loop with a loop variable of const ref type where the type of the variable does not match the one returned by the iterator. This means that an implicit conversion happens, which can for example result in expensive deep copies.")]
    [ClangCheck(true)]
    public bool PerformanceImplicitConversionInLoop { get; set; }


    [Category("Checks")]
    [DisplayName("performance-inefficient-algorithm")]
    [Description("Warns on inefficient use of STL algorithms on associative containers.")]
    [ClangCheck(true)]
    public bool PerformanceInefficientAlgorithm { get; set; }


    [Category("Checks")]
    [DisplayName("performance-inefficient-string-concatenation")]
    [Description("This check warns about the performance overhead arising from concatenating strings using the operator+, for instance:")]
    [ClangCheck(true)]
    public bool PerformanceInefficientStringConcatenation { get; set; }


    [Category("Checks")]
    [DisplayName("performance-inefficient-vector-operation")]
    [Description("Finds possible inefficient std::vector operations (e.g. push_back, emplace_back) that may cause unnecessary memory reallocations.")]
    [ClangCheck(true)]
    public bool PerformanceInefficientVectorOperation { get; set; }


    [Category("Checks")]
    [DisplayName("performance-move-const-arg")]
    [Description("The check warns")]
    [ClangCheck(true)]
    public bool PerformanceMoveConstArg { get; set; }


    [Category("Checks")]
    [DisplayName("performance-move-constructor-init")]
    [Description("“cert-oop11-cpp” redirects here as an alias for this check.")]
    [ClangCheck(true)]
    public bool PerformanceMoveConstructorInit { get; set; }


    [Category("Checks")]
    [DisplayName("performance-noexcept-move-constructor")]
    [Description("The check flags user-defined move constructors and assignment operators not marked with noexcept or marked with noexcept(expr) where expr evaluates to false (but is not a false literal itself).")]
    [ClangCheck(true)]
    public bool PerformanceNoexceptMoveConstructor { get; set; }


    [Category("Checks")]
    [DisplayName("performance-type-promotion-in-math-fn")]
    [Description("Finds calls to C math library functions (from math.h or, in C++, cmath) with implicit float to double promotions.")]
    [ClangCheck(true)]
    public bool PerformanceTypePromotionInMathFn { get; set; }


    [Category("Checks")]
    [DisplayName("performance-unnecessary-copy-initialization")]
    [Description("Finds local variable declarations that are initialized using the copy constructor of a non-trivially-copyable type but it would suffice to obtain a const reference.")]
    [ClangCheck(true)]
    public bool PerformanceUnnecessaryCopyInitialization { get; set; }


    [Category("Checks")]
    [DisplayName("performance-unnecessary-value-param")]
    [Description("Flags value parameter declarations of expensive to copy types that are copied for each invocation but it would suffice to pass them by const reference.")]
    [ClangCheck(true)]
    public bool PerformanceUnnecessaryValueParam { get; set; }


    [Category("Checks")]
    [DisplayName("portability-simd-intrinsics")]
    [Description("Finds SIMD intrinsics calls and suggests std::experimental::simd (P0214) alternatives.")]
    [ClangCheck(true)]
    public bool PortabilitySimdIntrinsics { get; set; }


    [Category("Checks")]
    [DisplayName("readability-avoid-const-params-in-decls")]
    [Description("Checks whether a function declaration has parameters that are top level const.")]
    [ClangCheck(true)]
    public bool ReadabilityAvoidConstParamsInDecls { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-braces-around-statements")]
    [Description("google-readability-braces-around-statements redirects here as an alias for this check.")]
    [ClangCheck(true)]
    public bool ReadabilityBracesAroundStatements { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-const-return-type")]
    [Description("Checks for functions with a const-qualified return type and recommends removal of the const keyword. Such use of const is usually superfluous, and can prevent valuable compiler optimizations.  Does not (yet) fix trailing return types.")]
    [ClangCheck(true)]
    public bool ReadabilityConstReturnType { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-container-size-empty")]
    [Description("Checks whether a call to the size() method can be replaced with a call to empty().")]
    [ClangCheck(true)]
    public bool ReadabilityContainerSizeEmpty { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-deleted-default")]
    [Description("Checks that constructors and assignment operators marked as = default are not actually deleted by the compiler.")]
    [ClangCheck(true)]
    public bool ReadabilityDeletedDefault { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-delete-null-pointer")]
    [Description("Checks the if statements where a pointer’s existence is checked and then deletes the pointer. The check is unnecessary as deleting a null pointer has no effect.")]
    [ClangCheck(true)]
    public bool ReadabilityDeleteNullPointer { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-else-after-return")]
    [Description("LLVM Coding Standards advises to reduce indentation where possible and where it makes understanding code easier. Early exit is one of the suggested enforcements of that. Please do not use else or else if after something that interrupts control flow - like return, break, continue, throw.")]
    [ClangCheck(true)]
    public bool ReadabilityElseAfterReturn { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-function-size")]
    [Description("google-readability-function-size redirects here as an alias for this check.")]
    [ClangCheck(true)]
    public bool ReadabilityFunctionSize { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-identifier-naming")]
    [Description("Checks for identifiers naming style mismatch.")]
    [ClangCheck(true)]
    public bool ReadabilityIdentifierNaming { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-implicit-bool-conversion")]
    [Description("This check can be used to find implicit conversions between built-in types and booleans. Depending on use case, it may simply help with readability of the code, or in some cases, point to potential bugs which remain unnoticed due to implicit conversions.")]
    [ClangCheck(true)]
    public bool ReadabilityImplicitBoolConversion { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-inconsistent-declaration-parameter-name")]
    [Description("Find function declarations which differ in parameter names.")]
    [ClangCheck(true)]
    public bool ReadabilityInconsistentDeclarationParameterName { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-isolate-declaration")]
    [Description("Detects local variable declarations declaring more than one variable and tries to refactor the code to one statement per declaration.")]
    [ClangCheck(true)]
    public bool ReadabilityIsolateDeclaration { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-magic-numbers")]
    [Description("Detects magic numbers, integer or floating point literals that are embedded in code and not introduced via constants or symbols.")]
    [ClangCheck(true)]
    public bool ReadabilityMagicNumbers { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-misleading-indentation")]
    [Description("Correct indentation helps to understand code. Mismatch of the syntactical structure and the indentation of the code may hide serious problems. Missing braces can also make it significantly harder to read the code, therefore it is important to use braces.")]
    [ClangCheck(true)]
    public bool ReadabilityMisleadingIndentation { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-misplaced-array-index")]
    [Description("This check warns for unusual array index syntax.")]
    [ClangCheck(true)]
    public bool ReadabilityMisplacedArrayIndex { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-named-parameter")]
    [Description("Find functions with unnamed arguments.")]
    [ClangCheck(true)]
    public bool ReadabilityNamedParameter { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-non-const-parameter")]
    [Description("The check finds function parameters of a pointer type that could be changed to point to a constant type instead.")]
    [ClangCheck(true)]
    public bool ReadabilityNonConstParameter { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-redundant-control-flow")]
    [Description("This check looks for procedures (functions returning no value) with return statements at the end of the function. Such return statements are redundant.")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantControlFlow { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-redundant-declaration")]
    [Description("Finds redundant variable and function declarations.")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantDeclaration { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-redundant-function-ptr-dereference")]
    [Description("Finds redundant dereferences of a function pointer.")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantFunctionPtrDereference { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-redundant-member-init")]
    [Description("Finds member initializations that are unnecessary because the same default constructor would be called if they were not present.")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantMemberInit { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-redundant-smartptr-get")]
    [Description("Find and remove redundant calls to smart pointer’s .get() method.")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantSmartptrGet { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-redundant-string-cstr")]
    [Description("Finds unnecessary calls to std::string::c_str() and std::string::data().")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantStringCstr { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-redundant-string-init")]
    [Description("Finds unnecessary string initializations.")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantStringInit { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-simplify-boolean-expr")]
    [Description("Looks for boolean expressions involving boolean constants and simplifies them to use the appropriate boolean expression directly.")]
    [ClangCheck(true)]
    public bool ReadabilitySimplifyBooleanExpr { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-simplify-subscript-expr")]
    [Description("This check simplifies subscript expressions. Currently this covers calling .data() and immediately doing an array subscript operation to obtain a single element, in which case simply calling operator[] suffice.")]
    [ClangCheck(true)]
    public bool ReadabilitySimplifySubscriptExpr { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-static-accessed-through-instance")]
    [Description("Checks for member expressions that access static members through instances, and replaces them with uses of the appropriate qualified-id.")]
    [ClangCheck(true)]
    public bool ReadabilityStaticAccessedThroughInstance { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-static-definition-in-anonymous-namespace")]
    [Description("Finds static function and variable definitions in anonymous namespace.")]
    [ClangCheck(true)]
    public bool ReadabilityStaticDefinitionInAnonymousNamespace { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-string-compare")]
    [Description("Finds string comparisons using the compare method.")]
    [ClangCheck(true)]
    public bool ReadabilityStringCompare { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-uniqueptr-delete-release")]
    [Description("Replace delete &lt;unique_ptr&gt;.release() with &lt;unique_ptr&gt; = nullptr. The latter is shorter, simpler and does not require use of raw pointer APIs.")]
    [ClangCheck(true)]
    public bool ReadabilityUniqueptrDeleteRelease { get; set; } = true;


    [Category("Checks")]
    [DisplayName("readability-uppercase-literal-suffix")]
    [Description("cert-dcl16-c redirects here as an alias for this check. By default, only the suffixes that begin with ‘l’ (“l”, “ll”, “lu”, “llu”, but not “u”, “ul”, “ull”) are diagnosed by that alias.")]
    [ClangCheck(true)]
    public bool ReadabilityUppercaseLiteralSuffix { get; set; } = true;


    [Category("Checks")]
    [DisplayName("security.FloatLoopCounter")]
    [Description("Warn on using a floating point value as a loop counter (CERT: FLP30-C, FLP30-CPP). ")]
    [ClangCheck(true)]
    public bool SecurityFloatLoopCounter { get; set; }


    [Category("Checks")]
    [DisplayName("security.insecureAPI.bcmp")]
    [Description("Warn on uses of the bcmp function. ")]
    [ClangCheck(true)]
    public bool SecurityinsecureAPIbcmp { get; set; }


    [Category("Checks")]
    [DisplayName("security.insecureAPI.bcopy")]
    [Description("Warn on uses of the bcopy function. ")]
    [ClangCheck(true)]
    public bool SecurityinsecureAPIbcopy { get; set; }


    [Category("Checks")]
    [DisplayName("security.insecureAPI.bzero")]
    [Description("Warn on uses of the bzero function. ")]
    [ClangCheck(true)]
    public bool SecurityinsecureAPIbzero { get; set; }


    [Category("Checks")]
    [DisplayName("security.insecureAPI.getpw")]
    [Description("Warn on uses of the getpw function. ")]
    [ClangCheck(true)]
    public bool SecurityinsecureAPIgetpw { get; set; }


    [Category("Checks")]
    [DisplayName("security.insecureAPI.gets")]
    [Description("Warn on uses of the gets function. ")]
    [ClangCheck(true)]
    public bool SecurityinsecureAPIgets { get; set; }


    [Category("Checks")]
    [DisplayName("security.insecureAPI.mkstemp")]
    [Description("Warn when mktemp, mkstemp, mkstemps or mkdtemp is passed fewer than 6 X's in the format string. ")]
    [ClangCheck(true)]
    public bool SecurityinsecureAPImkstemp { get; set; }


    [Category("Checks")]
    [DisplayName("security.insecureAPI.mktemp")]
    [Description("Warn on uses of the mktemp function. ")]
    [ClangCheck(true)]
    public bool SecurityinsecureAPImktemp { get; set; }


    [Category("Checks")]
    [DisplayName("security.insecureAPI.rand")]
    [Description("Warn on uses of inferior random number generating functions (only if arc4random function is available): drand48 erand48 jrand48 lcong48 lrand48 mrand48 nrand48 random rand_r ")]
    [ClangCheck(true)]
    public bool SecurityinsecureAPIrand { get; set; }


    [Category("Checks")]
    [DisplayName("security.insecureAPI.strcpy")]
    [Description("Warn on uses of the strcpy and strcat functions. ")]
    [ClangCheck(true)]
    public bool SecurityinsecureAPIstrcpy { get; set; }


    [Category("Checks")]
    [DisplayName("security.insecureAPI.UncheckedReturn")]
    [Description("Warn on uses of functions whose return values must be always checked: setuid setgid seteuid setegid setreuid setregid ")]
    [ClangCheck(true)]
    public bool SecurityinsecureAPIUncheckedReturn { get; set; }


    [Category("Checks")]
    [DisplayName("security.insecureAPI.vfork")]
    [Description("Warn on uses of the vfork function. ")]
    [ClangCheck(true)]
    public bool SecurityinsecureAPIvfork { get; set; }


    [Category("Checks")]
    [DisplayName("unix.API")]
    [Description("Check calls to various UNIX/POSIX functions: open pthread_once calloc malloc realloc alloca // Currently the check is performed for apple targets only. void test(const char *path) {   int fd = open(path, O_CREAT);     // warn: call to 'open' requires a third argument when the     // 'O_CREAT' flag is set } void f(); void test() {   pthread_once_t pred = {0x30B1BCBA, {0}};   pthread_once(&amp;pred, f);     // warn: call to 'pthread_once' uses the local variable } void test() {   void *p = malloc(0); // warn: allocation size of 0 bytes } void test() {   void *p = calloc(0, 42); // warn: allocation size of 0 bytes } void test() {   void *p = malloc(1);   p = realloc(p, 0); // warn: allocation size of 0 bytes } void test() {   void *p = alloca(0); // warn: allocation size of 0 bytes } void test() {   void *p = valloc(0); // warn: allocation size of 0 bytes } ")]
    [ClangCheck(true)]
    public bool UnixAPI { get; set; }


    [Category("Checks")]
    [DisplayName("unix.cstring.BadSizeArg")]
    [Description("Check the size argument passed to strncat for common erroneous patterns. Use -Wno-strncat-size compiler option to mute other strncat-related compiler warnings. ")]
    [ClangCheck(true)]
    public bool UnixcstringBadSizeArg { get; set; }


    [Category("Checks")]
    [DisplayName("unix.cstring.NullArg")]
    [Description("Check for null pointers being passed as arguments to C string functions: strlen strnlen strcpy strncpy strcat strncat strcmp strncmp strcasecmp strncasecmp ")]
    [ClangCheck(true)]
    public bool UnixcstringNullArg { get; set; }


    [Category("Checks")]
    [DisplayName("unix.Malloc")]
    [Description("Check for memory leaks, double free, and use-after-free and offset problems involving malloc. ")]
    [ClangCheck(true)]
    public bool UnixMalloc { get; set; }


    [Category("Checks")]
    [DisplayName("unix.MallocSizeof")]
    [Description("Check for dubious malloc, calloc or realloc arguments involving sizeof. ")]
    [ClangCheck(true)]
    public bool UnixMallocSizeof { get; set; }


    [Category("Checks")]
    [DisplayName("unix.MismatchedDeallocator")]
    [Description("Check for mismatched deallocators (e.g. passing a pointer allocating with new to free()). ")]
    [ClangCheck(true)]
    public bool UnixMismatchedDeallocator { get; set; }


    [Category("Checks")]
    [DisplayName("unix.Vfork")]
    [Description("Check for proper usage of vfork ")]
    [ClangCheck(true)]
    public bool UnixVfork { get; set; }


    [Category("Checks")]
    [DisplayName("zircon-temporary-objects")]
    [Description("Warns on construction of specific temporary objects in the Zircon kernel. If the object should be flagged, If the object should be flagged, the fully qualified type name must be explicitly passed to the check.")]
    [ClangCheck(true)]
    public bool ZirconTemporaryObjects { get; set; }


    protected override IWin32Window Window
    {
      get
      {
        ElementHost elementHost = new ElementHost();
        mUserControl = new ClangTidyPredefinedChecksOptionsUserControl(this);

        elementHost.Child = mUserControl;
        return elementHost;
      }
    }


    #endregion


    #region Methods


    #region Protected Methods


    protected override void OnDeactivate(CancelEventArgs e)
    {
      mUserControl.CleanQuickSearch();
    }


    #endregion


    #region DialogPage Save and Load implementation 


    public override void SaveSettingsToStorage()
    {
      var updatedConfig = new ClangTidyPredefinedChecksOptions();

      foreach (var prop in updatedConfig.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
      {
        Type cfgType = this.GetType();
        var value = cfgType.GetProperty(prop.Name).GetValue(this);

        if (value != null)
          prop.SetValue(updatedConfig, value);
      }

      string path = mSettingsPathBuilder.GetPath(kGeneralSettingsFileName);
      SaveToFile(path, updatedConfig);
    }

    public override void LoadSettingsFromStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kGeneralSettingsFileName);
      var loadedConfig = LoadFromFile(path);

      foreach (var prop in this.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
      {
        Type cfgType = loadedConfig.GetType();
        var value = cfgType.GetProperty(prop.Name).GetValue(loadedConfig);

        if (value != null)
          prop.SetValue(this, value);
      }

    }

    public override void ResetSettings()
    {
      SettingsHandler.CopySettingsProperties(new ClangTidyPredefinedChecksOptionsView(), SettingsProvider.TidyPredefinedChecks);
      SaveSettingsToStorage();
      LoadSettingsFromStorage();
    }

    #endregion


    #endregion

  }
}
