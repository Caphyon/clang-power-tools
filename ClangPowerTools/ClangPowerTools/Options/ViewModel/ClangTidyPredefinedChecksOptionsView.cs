using ClangPowerTools.Options.Model;
using ClangPowerTools.Options.View;
using System;
using System.ComponentModel;
using System.IO;
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

    #endregion

    #region Properties

    [Category("Checks")]
    [DisplayName("android-cloexec-creat")]
    [Description("The usage of creat() is not recommended, it’s better to use open().  ")]
    [ClangCheck(true)]
    public bool AndroidCloexecCreat { get; set; }

    [Category("Checks")]
    [DisplayName("android-cloexec-fopen")]
    [Description("fopen() should include e in their mode string; so re would be valid. This is equivalent to having set FD_CLOEXEC on that descriptor.  ")]
    [ClangCheck(true)]
    public bool AndroidCloexecFopen { get; set; }

    [Category("Checks")]
    [DisplayName("android-cloexec-open")]
    [Description("A common source of security bugs is code that opens a file without using the O_CLOEXEC flag.  Without that flag, an opened sensitive file would remain open across a fork+exec to a lower-privileged SELinux domain, leaking that sensitive data. Open-like functions including open(), openat(), and open64() should include O_CLOEXEC in their flags argument.  ")]
    [ClangCheck(true)]
    public bool AndroidCloexecOpen { get; set; }

    [Category("Checks")]
    [DisplayName("android-cloexec-socket")]
    [Description("socket() should include SOCK_CLOEXEC in its type argument to avoid the file descriptor leakage. Without this flag, an opened sensitive file would remain open across a fork+exec to a lower-privileged SELinux domain.  ")]
    [ClangCheck(true)]
    public bool AndroidCloexecSocket { get; set; }

    [Category("Checks")]
    [DisplayName("boost-use-to-string")]
    [Description("This check finds conversion from integer type like int to std::string or std::wstring using boost::lexical_cast, and replace it with calls to std::to_string and std::to_wstring. It doesn’t replace conversion from floating points despite the to_string overloads, because it would change the behaviour. ")]
    [ClangCheck(true)]
    public bool BoostUseToString { get; set; }

    [Category("Checks")]
    [DisplayName("bugprone-suspicious-memset-usage")]
    [Description("This check finds memset() calls with potential mistakes in their arguments. Considering the function as void* memset(void* destination, int fill_value, size_t byte_count), the following cases are covered: Case 1: Fill value is a character ``‘0’`` Filling up a memory area with ASCII code 48 characters is not customary, possibly integer zeroes were intended instead. The check offers a replacement of '0' with 0. Memsetting character pointers with '0' is allowed. Case 2: Fill value is truncated Memset converts fill_value to unsigned char before using it. If fill_value is out of unsigned character range, it gets truncated and memory will not contain the desired pattern. Case 3: Byte count is zero Calling memset with a literal zero in its byte_count argument is likely to be unintended and swapped with fill_value. The check offers to swap these two arguments. Corresponding cpplint.py check name: runtime/memset.  ")]
    [ClangCheck(true)]
    public bool BugproneSuspiciousMemsetUsage { get; set; }

    [Category("Checks")]
    [DisplayName("bugprone-undefined-memory-manipulation")]
    [Description("Finds calls of memory manipulation functions memset(), memcpy() and memmove() on not TriviallyCopyable objects resulting in undefined behavior. ")]
    [ClangCheck(true)]
    public bool BugproneUndefinedMemoryManipulation { get; set; }

    [Category("Checks")]
    [DisplayName("cert-dcl03-c")]
    [Description("The cert-dcl03-c check is an alias, please see misc-static-assert for more information. ")]
    [ClangCheck(true)]
    public bool CertDcl03C { get; set; }

    [Category("Checks")]
    [DisplayName("cert-dcl21-cpp")]
    [Description("This check flags postfix operator++ and operator-- declarations if the return type is not a const object. This also warns if the return type is a reference type. This check corresponds to the CERT C++ Coding Standard recommendation DCL21-CPP. Overloaded postfix increment and decrement operators should return a const object. ")]
    [ClangCheck(true)]
    public bool CertDcl21Cpp { get; set; }

    [Category("Checks")]
    [DisplayName("cert-dcl50-cpp")]
    [Description("This check flags all function definitions (but not declarations) of C-style variadic functions. This check corresponds to the CERT C++ Coding Standard rule DCL50-CPP. Do not define a C-style variadic function. ")]
    [ClangCheck(true)]
    public bool CertDcl50Cpp { get; set; }

    [Category("Checks")]
    [DisplayName("cert-dcl54-cpp")]
    [Description("The cert-dcl54-cpp check is an alias, please see misc-new-delete-overloads for more information. ")]
    [ClangCheck(true)]
    public bool CertDcl54Cpp { get; set; }

    [Category("Checks")]
    [DisplayName("cert-dcl58-cpp")]
    [Description("Modification of the std or posix namespace can result in undefined behavior. This check warns for such modifications.  This check corresponds to the CERT C++ Coding Standard rule DCL58-CPP. Do not modify the standard namespaces. ")]
    [ClangCheck(true)]
    public bool CertDcl58Cpp { get; set; }

    [Category("Checks")]
    [DisplayName("cert-dcl59-cpp")]
    [Description("The cert-dcl59-cpp check is an alias, please see google-build-namespaces for more information. ")]
    [ClangCheck(true)]
    public bool CertDcl59Cpp { get; set; }

    [Category("Checks")]
    [DisplayName("cert-env33-c")]
    [Description("This check flags calls to system(), popen(), and _popen(), which execute a command processor. It does not flag calls to system() with a null pointer argument, as such a call checks for the presence of a command processor but does not actually attempt to execute a command. This check corresponds to the CERT C Coding Standard rule ENV33-C. Do not call system(). ")]
    [ClangCheck(true)]
    public bool CertEnv33C { get; set; }

    [Category("Checks")]
    [DisplayName("cert-err09-cpp")]
    [Description("The cert-err09-cpp check is an alias, please see misc-throw-by-value-catch-by-reference for more information. ")]
    [ClangCheck(true)]
    public bool CertErr09Cpp { get; set; }

    [Category("Checks")]
    [DisplayName("cert-err34-c")]
    [Description("This check flags calls to string-to-number conversion functions that do not verify the validity of the conversion, such as atoi() or scanf(). It does not flag calls to strtol(), or other, related conversion functions that do perform better error checking. This check corresponds to the CERT C Coding Standard rule ERR34-C. Detect errors when converting a string to a number. ")]
    [ClangCheck(true)]
    public bool CertErr34C { get; set; }

    [Category("Checks")]
    [DisplayName("cert-err52-cpp")]
    [Description("This check flags all call expressions involving setjmp() and longjmp(). This check corresponds to the CERT C++ Coding Standard rule ERR52-CPP. Do not use setjmp() or longjmp(). ")]
    [ClangCheck(true)]
    public bool CertErr52Cpp { get; set; }

    [Category("Checks")]
    [DisplayName("cert-err58-cpp")]
    [Description("This check flags all static or thread_local variable declarations where the initializer for the object may throw an exception. This check corresponds to the CERT C++ Coding Standard rule ERR58-CPP. Handle all exceptions thrown before main() begins executing. ")]
    [ClangCheck(true)]
    public bool CertErr58Cpp { get; set; }

    [Category("Checks")]
    [DisplayName("cert-err60-cpp")]
    [Description("This check flags all throw expressions where the exception object is not nothrow copy constructible. This check corresponds to the CERT C++ Coding Standard rule ERR60-CPP. Exception objects must be nothrow copy constructible. ")]
    [ClangCheck(true)]
    public bool CertErr60Cpp { get; set; }

    [Category("Checks")]
    [DisplayName("cert-err61-cpp")]
    [Description("The cert-err61-cpp check is an alias, please see misc-throw-by-value-catch-by-reference for more information. ")]
    [ClangCheck(true)]
    public bool CertErr61Cpp { get; set; }

    [Category("Checks")]
    [DisplayName("cert-fio38-c")]
    [Description("The cert-fio38-c check is an alias, please see misc-non-copyable-objects for more information. ")]
    [ClangCheck(true)]
    public bool CertFio38C { get; set; }

    [Category("Checks")]
    [DisplayName("cert-flp30-c")]
    [Description("This check flags for loops where the induction expression has a floating-point type. This check corresponds to the CERT C Coding Standard rule FLP30-C. Do not use floating-point variables as loop counters. ")]
    [ClangCheck(true)]
    public bool CertFlp30C { get; set; }

    [Category("Checks")]
    [DisplayName("cert-msc30-c")]
    [Description("The cert-msc30-c check is an alias, please see cert-msc50-cpp for more information. ")]
    [ClangCheck(true)]
    public bool CertMsc30C { get; set; }

    [Category("Checks")]
    [DisplayName("cert-msc50-cpp")]
    [Description("Pseudorandom number generators use mathematical algorithms to produce a sequence of numbers with good statistical properties, but the numbers produced are not genuinely random. The std::rand() function takes a seed (number), runs a mathematical operation on it and returns the result. By manipulating the seed the result can be predictable. This check warns for the usage of std::rand(). ")]
    [ClangCheck(true)]
    public bool CertMsc50Cpp { get; set; }

    [Category("Checks")]
    [DisplayName("cert-oop11-cpp")]
    [Description("The cert-oop11-cpp check is an alias, please see misc-move-constructor-init for more information. ")]
    [ClangCheck(true)]
    public bool CertOop11Cpp { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-apiModeling.google.GTest")]
    [ClangCheck(true)]
    public bool ClangAnalyzerApimodelinggoogleGTest { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.builtin.BuiltinFunctions")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCorebuiltinBuiltinFunctions { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.builtin.NoReturnFunctions")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCorebuiltinNoReturnFunctions { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.CallAndMessage")]
    [Description("Check for logical errors for function calls and Objective-C message expressions (e.g., uninitialized arguments, null function pointers).")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreCallAndMessage { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.DivideZero")]
    [Description("Check for division by zero.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreDivideZero { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.DynamicTypePropagation")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreDynamicTypePropagation { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.NonNullParamChecker")]
    [Description("Check for null pointers passed as arguments to a function whose arguments are marked with the nonnull attribute.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreNonNullParamChecker { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.NullDereference")]
    [Description("Check for dereferences of null pointers.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreNullDereference { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.StackAddressEscape")]
    [Description("Check that addresses of stack memory do not escape the function.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreStackAddressEscape { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.UndefinedBinaryOperatorResult")]
    [Description("Check for undefined results of binary operators.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreUndefinedBinaryOperatorResult { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.uninitialized.ArraySubscript")]
    [Description("Check for uninitialized values used as array subscripts.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreuninitializedArraySubscript { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.uninitialized.Assign")]
    [Description("Check for assigning uninitialized values.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreuninitializedAssign { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.uninitialized.Branch")]
    [Description("Check for uninitialized values used as branch conditions.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreuninitializedBranch { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.uninitialized.CapturedBlockVariable")]
    [Description("Check for blocks that capture uninitialized values.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreuninitializedCapturedBlockVariable { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.uninitialized.UndefReturn")]
    [Description("Check for uninitialized values being returned to the caller.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreuninitializedUndefReturn { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-core.VLASize")]
    [Description("Check for declarations of VLA of undefined or zero size.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCoreVLASize { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-cplusplus.NewDelete")]
    [Description("Check for double-free, use-after-free and offset problems involving C++  delete.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCplusplusNewDelete { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-cplusplus.NewDeleteLeaks")]
    [Description("Check for memory leaks. Traces memory managed by new/ delete.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCplusplusNewDeleteLeaks { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-cplusplus.SelfAssignment")]
    [ClangCheck(true)]
    public bool ClangAnalyzerCplusplusSelfAssignment { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-deadcode.DeadStores")]
    [Description("Check for values stored to variables that are never read afterwards.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerDeadcodeDeadStores { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-llvm.Conventions")]
    [ClangCheck(true)]
    public bool ClangAnalyzerLlvmConventions { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-nullability.NullableDereferenced")]
    [Description("Warns when a nullable pointer is dereferenced.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerNullabilityNullableDereferenced { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-nullability.NullablePassedToNonnull")]
    [Description("Warns when a nullable pointer is passed to a pointer which has a _Nonnull type.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerNullabilityNullablePassedToNonnull { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-nullability.NullableReturnedFromNonnull")]
    [ClangCheck(true)]
    public bool ClangAnalyzerNullabilityNullableReturnedFromNonnull { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-nullability.NullPassedToNonnull")]
    [Description("Warns when a null pointer is passed to a pointer which has a  _Nonnull type.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerNullabilityNullPassedToNonnull { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-nullability.NullReturnedFromNonnull")]
    [Description("Warns when a null pointer is returned from a function that has  _Nonnull return type.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerNullabilityNullReturnedFromNonnull { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-optin.cplusplus.VirtualCall")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOptincplusplusVirtualCall { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-optin.mpi.MPI-Checker")]
    [Description("Checks MPI code")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOptinmpiMPIChecker { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-optin.osx.cocoa.localizability.EmptyLocalizationContextChecker")]
    [Description("Check that NSLocalizedString macros include a comment for context.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOptinosxcocoalocalizabilityEmptyLocalizationContextChecker { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-optin.osx.cocoa.localizability.NonLocalizedStringChecker")]
    [Description("Warns about uses of non-localized NSStrings passed to UI methods  expecting localized NSStrings")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOptinosxcocoalocalizabilityNonLocalizedStringChecker { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-optin.performance.Padding")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOptinperformancePadding { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-optin.portability.UnixAPI")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOptinportabilityUnixAPI { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.API")]
    [Description("Check for proper uses of various Apple APIs: dispatch_once")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxAPI { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.AtSync")]
    [Description("Check for nil pointers used as mutexes for @synchronized.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaAtSync { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.ClassRelease")]
    [Description("Check for sending retain, release, or  autorelease directly to a class.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaClassRelease { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.Dealloc")]
    [Description("Warn about Objective-C classes that lack a correct implementation  of -dealloc. ")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaDealloc { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.IncompatibleMethodTypes")]
    [Description("Check for an incompatible type signature when overriding an Objective-C method.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaIncompatibleMethodTypes { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.Loops")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaLoops { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.MissingSuperCall")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaMissingSuperCall { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.NilArg")]
    [Description("Check for prohibited nil arguments in specific Objective-C method calls: - caseInsensitiveCompare: - compare: - compare:options: - compare:options:range: - compare:options:range:locale: - componentsSeparatedByCharactersInSet: - initWithFormat:")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaNilArg { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.NonNilReturnValue")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaNonNilReturnValue { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.NSAutoreleasePool")]
    [Description("Warn for suboptimal uses of NSAutoreleasePool in Objective-C GC mode (-fobjc-gc compiler option).")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaNSAutoreleasePool { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.NSError")]
    [Description("Check usage of NSError** parameters.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaNSError { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.ObjCGenerics")]
    [Description("Check for type errors when using Objective-C generics")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaObjCGenerics { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.RetainCount")]
    [Description("Check for leaks and violations of the Cocoa Memory Management rules.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaRetainCount { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.SelfInit")]
    [Description("Check that self is properly initialized inside an initializer method.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaSelfInit { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.SuperDealloc")]
    [Description("Warn about improper use of '[super dealloc]' in Objective-C")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaSuperDealloc { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.UnusedIvars")]
    [Description("Warn about private ivars that are never used.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaUnusedIvars { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.cocoa.VariadicMethodTypes")]
    [Description("Check for passing non-Objective-C types to variadic collection initialization methods that expect only Objective-C types.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcocoaVariadicMethodTypes { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.coreFoundation.CFError")]
    [Description("Check usage of CFErrorRef* parameters.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcoreFoundationCFError { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.coreFoundation.CFNumber")]
    [Description("Check for improper uses of CFNumberCreate.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcoreFoundationCFNumber { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.coreFoundation.CFRetainRelease")]
    [Description("Check for null arguments to CFRetain, CFRelease, CFMakeCollectable.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcoreFoundationCFRetainRelease { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.coreFoundation.containers.OutOfBounds")]
    [Description("Checks for index out-of-bounds when using CFArray API.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcoreFoundationcontainersOutOfBounds { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.coreFoundation.containers.PointerSizedValues")]
    [Description("Warns if CFArray, CFDictionary, CFSet are created with non-pointer-size values.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxcoreFoundationcontainersPointerSizedValues { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.NumberObjectConversion")]
    [Description("Check for erroneous conversions of objects representing numbers  into numbers")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxNumberObjectConversion { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.ObjCProperty")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxObjCProperty { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-osx.SecKeychainAPI")]
    [Description("Check for improper uses of the Security framework's Keychain APIs: SecKeychainItemCopyContent SecKeychainFindGenericPassword SecKeychainFindInternetPassword SecKeychainItemFreeContent SecKeychainItemCopyAttributesAndData SecKeychainItemFreeAttributesAndData")]
    [ClangCheck(true)]
    public bool ClangAnalyzerOsxSecKeychainAPI { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-security.FloatLoopCounter")]
    [Description("Warn on using a floating point value as a loop counter (CERT: FLP30-C, FLP30-CPP).")]
    [ClangCheck(true)]
    public bool ClangAnalyzerSecurityFloatLoopCounter { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-security.insecureAPI.getpw")]
    [Description("Warn on uses of the getpw function.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerSecurityinsecureAPIgetpw { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-security.insecureAPI.gets")]
    [Description("Warn on uses of the gets function.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerSecurityinsecureAPIgets { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-security.insecureAPI.mkstemp")]
    [Description("Warn when mktemp, mkstemp, mkstemps or mkdtemp is passed fewer than 6 X's in the format string.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerSecurityinsecureAPImkstemp { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-security.insecureAPI.mktemp")]
    [Description("Warn on uses of the mktemp function.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerSecurityinsecureAPImktemp { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-security.insecureAPI.rand")]
    [Description("Warn on uses of inferior random number generating functions (only if arc4random function is available): drand48 erand48 jrand48 lcong48 lrand48 mrand48 nrand48 random rand_r")]
    [ClangCheck(true)]
    public bool ClangAnalyzerSecurityinsecureAPIrand { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-security.insecureAPI.strcpy")]
    [Description("Warn on uses of the strcpy and strcat functions.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerSecurityinsecureAPIstrcpy { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-security.insecureAPI.UncheckedReturn")]
    [Description("Warn on uses of functions whose return values must be always checked: setuid setgid seteuid setegid setreuid setregid")]
    [ClangCheck(true)]
    public bool ClangAnalyzerSecurityinsecureAPIUncheckedReturn { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-security.insecureAPI.vfork")]
    [Description("Warn on uses of the vfork function.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerSecurityinsecureAPIvfork { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-unix.API")]
    [Description("Check calls to various UNIX/POSIX functions: open pthread_once calloc malloc realloc alloca ")]
    [ClangCheck(true)]
    public bool ClangAnalyzerUnixAPI { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-unix.cstring.BadSizeArg")]
    [Description("Check the size argument passed to strncat for common erroneous patterns. Use -Wno-strncat-size compiler option to mute other strncat-related compiler warnings. ")]
    [ClangCheck(true)]
    public bool ClangAnalyzerUnixcstringBadSizeArg { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-unix.cstring.NullArg")]
    [Description("Check for null pointers being passed as arguments to C string functions: strlen strnlen strcpy strncpy strcat strncat strcmp strncmp strcasecmp strncasecmp")]
    [ClangCheck(true)]
    public bool ClangAnalyzerUnixcstringNullArg { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-unix.Malloc")]
    [Description("Check for memory leaks, double free, and use-after-free and offset problems involving malloc.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerUnixMalloc { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-unix.MallocSizeof")]
    [Description("Check for dubious malloc, calloc or realloc arguments involving sizeof.")]
    [ClangCheck(true)]
    public bool ClangAnalyzerUnixMallocSizeof { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-unix.MismatchedDeallocator")]
    [Description("Check for mismatched deallocators (e.g. passing a pointer allocating with new to free()).")]
    [ClangCheck(true)]
    public bool ClangAnalyzerUnixMismatchedDeallocator { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-unix.StdCLibraryFunctions")]
    [ClangCheck(true)]
    public bool ClangAnalyzerUnixStdCLibraryFunctions { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-unix.Vfork")]
    [Description("Check for proper usage of vfork")]
    [ClangCheck(true)]
    public bool ClangAnalyzerUnixVfork { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-valist.CopyToSelf")]
    [ClangCheck(true)]
    public bool ClangAnalyzerValistCopyToSelf { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-valist.Uninitialized")]
    [ClangCheck(true)]
    public bool ClangAnalyzerValistUninitialized { get; set; }

    [Category("Checks")]
    [DisplayName("clang-analyzer-valist.Unterminated")]
    [ClangCheck(true)]
    public bool ClangAnalyzerValistUnterminated { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-c-copy-assignment-signature")]
    [Description("The cppcoreguidelines-c-copy-assignment-signature check is an alias, please see misc-unconventional-assign-operator for more information. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesCCopyAssignmentSignature { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-interfaces-global-init")]
    [Description("This check flags initializers of globals that access extern objects, and therefore can lead to order-of-initialization problems. This rule is part of the “Interfaces” profile of the C++ Core Guidelines, see https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#Ri-global-init Note that currently this does not flag calls to non-constexpr functions, and therefore globals could still be accessed from functions themselves. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesInterfacesGlobalInit { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-no-malloc")]
    [Description("This check handles C-Style memory management using malloc(), realloc(), calloc() and free(). It warns about its use and tries to suggest the use of an appropriate RAII object. Furthermore, it can be configured to check against a user-specified list of functions that are used for memory management (e.g. posix_memalign()). See C++ Core Guidelines. There is no attempt made to provide fix-it hints, since manual resource management isn’t easily transformed automatically into RAII. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesNoMalloc { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-bounds-array-to-pointer-decay")]
    [Description("This check flags all array to pointer decays. Pointers should not be used as arrays. span<T> is a bounds-checked, safe alternative to using pointers to access arrays. This rule is part of the “Bounds safety” profile of the C++ Core Guidelines, see https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#Pro-bounds-decay. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProBoundsArrayToPointerDecay { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-bounds-constant-array-index")]
    [Description("This check flags all array subscript expressions on static arrays and std::arrays that either do not have a constant integer expression index or are out of bounds (for std::array). For out-of-bounds checking of static arrays, see the -Warray-bounds Clang diagnostic. This rule is part of the “Bounds safety” profile of the C++ Core Guidelines, see https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#Pro-bounds-arrayindex. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProBoundsConstantArrayIndex { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-bounds-pointer-arithmetic")]
    [Description("This check flags all usage of pointer arithmetic, because it could lead to an invalid pointer. Subtraction of two pointers is not flagged by this check. Pointers should only refer to single objects, and pointer arithmetic is fragile and easy to get wrong. span<T> is a bounds-checked, safe type for accessing arrays of data. This rule is part of the “Bounds safety” profile of the C++ Core Guidelines, see https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#Pro-bounds-arithmetic. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProBoundsPointerArithmetic { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-const-cast")]
    [Description("This check flags all uses of const_cast in C++ code. Modifying a variable that was declared const is undefined behavior, even with const_cast. This rule is part of the “Type safety” profile of the C++ Core Guidelines, see https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#Pro-type-constcast. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeConstCast { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-cstyle-cast")]
    [Description("This check flags all use of C-style casts that perform a static_cast downcast, const_cast, or reinterpret_cast. Use of these casts can violate type safety and cause the program to access a variable that is actually of type X to be accessed as if it were of an unrelated type Z. Note that a C-style (T)expression cast means to perform the first of the following that is possible: a const_cast, a static_cast, a static_cast followed by a const_cast, a reinterpret_cast, or a reinterpret_cast followed by a const_cast. This rule bans (T)expression only when used to perform an unsafe cast. This rule is part of the “Type safety” profile of the C++ Core Guidelines, see https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#Pro-type-cstylecast. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeCstyleCast { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-member-init")]
    [Description("The check flags user-defined constructor definitions that do not initialize all fields that would be left in an undefined state by default construction, e.g. builtins, pointers and record types without user-provided default constructors containing at least one such type. If these fields aren’t initialized, the constructor will leave some of the memory in an undefined state. For C++11 it suggests fixes to add in-class field initializers. For older versions it inserts the field initializers into the constructor initializer list. It will also initialize any direct base classes that need to be zeroed in the constructor initializer list. The check takes assignment of fields in the constructor body into account but generates false positives for fields initialized in methods invoked in the constructor body. The check also flags variables with automatic storage duration that have record types without a user-provided constructor and are not initialized. The suggested fix is to zero initialize the variable via {} for C++11 and beyond or = {} for older language versions. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeMemberInit { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-reinterpret-cast")]
    [Description("This check flags all uses of reinterpret_cast in C++ code. Use of these casts can violate type safety and cause the program to access a variable that is actually of type X to be accessed as if it were of an unrelated type Z. This rule is part of the “Type safety” profile of the C++ Core Guidelines, see https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#Pro-type-reinterpretcast. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeReinterpretCast { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-static-cast-downcast")]
    [Description("This check flags all usages of static_cast, where a base class is casted to a derived class. In those cases, a fix-it is provided to convert the cast to a dynamic_cast. Use of these casts can violate type safety and cause the program to access a variable that is actually of type X to be accessed as if it were of an unrelated type Z. This rule is part of the “Type safety” profile of the C++ Core Guidelines, see https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#Pro-type-downcast. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeStaticCastDowncast { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-union-access")]
    [Description("This check flags all access to members of unions. Passing unions as a whole is not flagged. Reading from a union member assumes that member was the last one written, and writing to a union member assumes another member with a nontrivial destructor had its destructor called. This is fragile because it cannot generally be enforced to be safe in the language and so relies on programmer discipline to get it right. This rule is part of the “Type safety” profile of the C++ Core Guidelines, see https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#Pro-type-unions. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeUnionAccess { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-pro-type-vararg")]
    [Description("This check flags all calls to c-style vararg functions and all use of va_arg. To allow for SFINAE use of vararg functions, a call is not flagged if a literal 0 is passed as the only vararg argument. Passing to varargs assumes the correct type will be read. This is fragile because it cannot generally be enforced to be safe in the language and so relies on programmer discipline to get it right. This rule is part of the “Type safety” profile of the C++ Core Guidelines, see https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#Pro-type-varargs. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesProTypeVararg { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-slicing")]
    [Description("Flags slicing of member variables or vtable. Slicing happens when copying a derived object into a base object: the members of the derived object (both member variables and virtual member functions) will be discarded. This can be misleading especially for member function slicing, for example: See the relevant C++ Core Guidelines sections for details: https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#es63-dont-slice https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#c145-access-polymorphic-objects-through-pointers-and-references ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesSlicing { get; set; }

    [Category("Checks")]
    [DisplayName("cppcoreguidelines-special-member-functions")]
    [Description("The check finds classes where some but not all of the special member functions are defined. By default the compiler defines a copy constructor, copy assignment operator, move constructor, move assignment operator and destructor. The default can be suppressed by explicit user-definitions. The relationship between which functions will be suppressed by definitions of other functions is complicated and it is advised that all five are defaulted or explicitly defined. Note that defining a function with = delete is considered to be a definition. This rule is part of the “Constructors, assignments, and destructors” profile of the C++ Core Guidelines, corresponding to rule C.21. See https://github.com/isocpp/CppCoreGuidelines/blob/master/CppCoreGuidelines.md#c21-if-you-define-or-delete-any-default-operation-define-or-delete-them-all. ")]
    [ClangCheck(true)]
    public bool CppcoreguidelinesSpecialMemberFunctions { get; set; }

    [Category("Checks")]
    [DisplayName("google-build-explicit-make-pair")]
    [Description("Check that make_pair‘s template arguments are deduced. G++ 4.6 in C++11 mode fails badly if make_pair‘s template arguments are specified explicitly, and such use isn’t intended in any case. Corresponding cpplint.py check name: build/explicit_make_pair. ")]
    [ClangCheck(true)]
    public bool GoogleBuildExplicitMakePair { get; set; }

    [Category("Checks")]
    [DisplayName("google-build-namespaces")]
    [Description("cert-dcl59-cpp redirects here as an alias for this check. Finds anonymous namespaces in headers. https://google.github.io/styleguide/cppguide.html#Namespaces Corresponding cpplint.py check name: build/namespaces. ")]
    [ClangCheck(true)]
    public bool GoogleBuildNamespaces { get; set; }

    [Category("Checks")]
    [DisplayName("google-build-using-namespace")]
    [Description("Finds using namespace directives. The check implements the following rule of the Google C++ Style Guide: Corresponding cpplint.py check name: build/namespaces. ")]
    [ClangCheck(true)]
    public bool GoogleBuildUsingNamespace { get; set; }

    [Category("Checks")]
    [DisplayName("google-default-arguments")]
    [Description("Checks that default arguments are not given for virtual methods. See https://google.github.io/styleguide/cppguide.html#Default_Arguments ")]
    [ClangCheck(true)]
    public bool GoogleDefaultArguments { get; set; }

    [Category("Checks")]
    [DisplayName("google-explicit-constructor")]
    [Description("Checks that constructors callable with a single argument and conversion operators are marked explicit to avoid the risk of unintentional implicit conversions. Consider this example: The function will return true, since the objects are implicitly converted to bool before comparison, which is unlikely to be the intent. The check will suggest inserting explicit before the constructor or conversion operator declaration. However, copy and move constructors should not be explicit, as well as constructors taking a single initializer_list argument. This code: will become See https://google.github.io/styleguide/cppguide.html#Explicit_Constructors ")]
    [ClangCheck(true)]
    public bool GoogleExplicitConstructor { get; set; }

    [Category("Checks")]
    [DisplayName("google-global-names-in-headers")]
    [Description("Flag global namespace pollution in header files. Right now it only triggers on using declarations and directives. The relevant style guide section is https://google.github.io/styleguide/cppguide.html#Namespaces. ")]
    [ClangCheck(true)]
    public bool GoogleGlobalNamesInHeaders { get; set; }

    [Category("Checks")]
    [DisplayName("google-readability-braces-around-statements")]
    [Description("The google-readability-braces-around-statements check is an alias, please see readability-braces-around-statements for more information. ")]
    [ClangCheck(true)]
    public bool GoogleReadabilityBracesAroundStatements { get; set; } = true;

    [Category("Checks")]
    [DisplayName("google-readability-casting")]
    [Description("Finds usages of C-style casts. https://google.github.io/styleguide/cppguide.html#Casting Corresponding cpplint.py check name: readability/casting. This check is similar to -Wold-style-cast, but it suggests automated fixes in some cases. The reported locations should not be different from the ones generated by -Wold-style-cast. ")]
    [ClangCheck(true)]
    public bool GoogleReadabilityCasting { get; set; } = true;

    [Category("Checks")]
    [DisplayName("google-readability-function-size")]
    [Description("The google-readability-function-size check is an alias, please see readability-function-size for more information. ")]
    [ClangCheck(true)]
    public bool GoogleReadabilityFunctionSize { get; set; } = true;

    [Category("Checks")]
    [DisplayName("google-readability-namespace-comments")]
    [Description("The google-readability-namespace-comments check is an alias, please see llvm-namespace-comment for more information. ")]
    [ClangCheck(true)]
    public bool GoogleReadabilityNamespaceComments { get; set; } = true;

    [Category("Checks")]
    [DisplayName("google-readability-redundant-smartptr-get")]
    [Description("The google-readability-redundant-smartptr-get check is an alias, please see readability-redundant-smartptr-get for more information. ")]
    [ClangCheck(true)]
    public bool GoogleReadabilityRedundantSmartptrGet { get; set; } = true;

    [Category("Checks")]
    [DisplayName("google-readability-todo")]
    [Description("Finds TODO comments without a username or bug number. The relevant style guide section is https://google.github.io/styleguide/cppguide.html#TODO_Comments. Corresponding cpplint.py check: readability/todo ")]
    [ClangCheck(true)]
    public bool GoogleReadabilityTodo { get; set; } = true;

    [Category("Checks")]
    [DisplayName("google-runtime-int")]
    [Description("Finds uses of short, long and long long and suggest replacing them with u?intXX(_t)?. The corresponding style guide rule: https://google.github.io/styleguide/cppguide.html#Integer_Types. Correspondig cpplint.py check: runtime/int. ")]
    [ClangCheck(true)]
    public bool GoogleRuntimeInt { get; set; }

    [Category("Checks")]
    [DisplayName("google-runtime-member-string-references")]
    [Description("Finds members of type const string&. const string reference members are generally considered unsafe as they can be created from a temporary quite easily. In the constructor call a string temporary is created from const char * and destroyed immediately after the call. This leaves around a dangling reference. This check emit warnings for both std::string and ::string const reference members. Corresponding cpplint.py check name: runtime/member_string_reference. ")]
    [ClangCheck(true)]
    public bool GoogleRuntimeMemberStringReferences { get; set; }

    [Category("Checks")]
    [DisplayName("google-runtime-operator")]
    [Description("Finds overloads of unary operator &. https://google.github.io/styleguide/cppguide.html#Operator_Overloading Corresponding cpplint.py check name: runtime/operator. ")]
    [ClangCheck(true)]
    public bool GoogleRuntimeOperator { get; set; }

    [Category("Checks")]
    [DisplayName("google-runtime-references")]
    [Description("Checks the usage of non-constant references in function parameters. The corresponding style guide rule: https://google.github.io/styleguide/cppguide.html#Reference_Arguments ")]
    [ClangCheck(true)]
    public bool GoogleRuntimeReferences { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-explicit-conversions")]
    [Description("This check is an alias for google-explicit-constructor. Used to enforce parts of rule 5.4.1. This check will enforce that constructors and conversion operators are marked explicit. Other forms of casting checks are implemented in other places. The following checks can be used to check for more forms of casting: ")]
    [ClangCheck(true)]
    public bool HicppExplicitConversions { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-function-size")]
    [Description("This check is an alias for readability-function-size. Useful to enforce multiple sections on function complexity. ")]
    [ClangCheck(true)]
    public bool HicppFunctionSize { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-invalid-access-moved")]
    [Description("This check is an alias for misc-use-after-move. Implements parts of the rule 8.4.1 to check if moved-from objects are accessed. ")]
    [ClangCheck(true)]
    public bool HicppInvalidAccessMoved { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-member-init")]
    [Description("This check is an alias for cppcoreguidelines-pro-type-member-init. Implements the check for rule 12.4.2 to initialize class members in the right order. ")]
    [ClangCheck(true)]
    public bool HicppMemberInit { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-named-parameter")]
    [Description("This check is an alias for readability-named-parameter. Implements rule 8.2.1. ")]
    [ClangCheck(true)]
    public bool HicppNamedParameter { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-new-delete-operators")]
    [Description("This check is an alias for misc-new-delete-overloads. Implements rule 12.3.1 to ensure the new and delete operators have the correct signature. ")]
    [ClangCheck(true)]
    public bool HicppNewDeleteOperators { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-no-assembler")]
    [Description("Check for assembler statements. No fix is offered. Inline assembler is forbidden by the High Intergrity C++ Coding Standard as it restricts the portability of code. ")]
    [ClangCheck(true)]
    public bool HicppNoAssembler { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-noexcept-move")]
    [Description("This check is an alias for misc-noexcept-moveconstructor. Checks rule 12.5.4 to mark move assignment and move construction noexcept. ")]
    [ClangCheck(true)]
    public bool HicppNoexceptMove { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-special-member-functions")]
    [Description("This check is an alias for cppcoreguidelines-special-member-functions. Checks that special member functions have the correct signature, according to rule 12.5.7. ")]
    [ClangCheck(true)]
    public bool HicppSpecialMemberFunctions { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-undelegated-constructor")]
    [Description("This check is an alias for misc-undelegated-constructor. Partially implements rule 12.4.5 to find misplaced constructor calls inside a constructor. ")]
    [ClangCheck(true)]
    public bool HicppUndelegatedConstructor { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-use-equals-default")]
    [Description("This check is an alias for modernize-use-equals-default. Implements rule 12.5.1 to explicitly default special member functions. ")]
    [ClangCheck(true)]
    public bool HicppUseEqualsDefault { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-use-equals-delete")]
    [Description("This check is an alias for modernize-use-equals-delete. Implements rule 12.5.1 to explicitly default or delete special member functions. ")]
    [ClangCheck(true)]
    public bool HicppUseEqualsDelete { get; set; }

    [Category("Checks")]
    [DisplayName("hicpp-use-override")]
    [Description("This check is an alias for modernize-use-override. Implements rule 10.2.1 to declare a virtual function override when overriding. ")]
    [ClangCheck(true)]
    public bool HicppUseOverride { get; set; }

    [Category("Checks")]
    [DisplayName("llvm-header-guard")]
    [Description("Finds and fixes header guards that do not adhere to LLVM style. ")]
    [ClangCheck(true)]
    public bool LlvmHeaderGuard { get; set; }

    [Category("Checks")]
    [DisplayName("llvm-include-order")]
    [Description("Checks the correct order of #includes. See http://llvm.org/docs/CodingStandards.html#include-style ")]
    [ClangCheck(true)]
    public bool LlvmIncludeOrder { get; set; }

    [Category("Checks")]
    [DisplayName("llvm-namespace-comment")]
    [Description("google-readability-namespace-comments redirects here as an alias for this check. Checks that long namespaces have a closing comment. http://llvm.org/docs/CodingStandards.html#namespace-indentation https://google.github.io/styleguide/cppguide.html#Namespaces ")]
    [ClangCheck(true)]
    public bool LlvmNamespaceComment { get; set; }

    [Category("Checks")]
    [DisplayName("llvm-twine-local")]
    [Description("Looks for local Twine variables which are prone to use after frees and should be generally avoided. ")]
    [ClangCheck(true)]
    public bool LlvmTwineLocal { get; set; }

    [Category("Checks")]
    [DisplayName("misc-argument-comment")]
    [Description("Checks that argument comments match parameter names. The check understands argument comments in the form /*parameter_name=*/ that are placed right before the argument. The check tries to detect typos and suggest automated fixes for them. ")]
    [ClangCheck(true)]
    public bool MiscArgumentComment { get; set; }

    [Category("Checks")]
    [DisplayName("misc-assert-side-effect")]
    [Description("Finds assert() with side effect. The condition of assert() is evaluated only in debug builds so a condition with side effect can cause different behavior in debug / release builds. ")]
    [ClangCheck(true)]
    public bool MiscAssertSideEffect { get; set; }

    [Category("Checks")]
    [DisplayName("misc-bool-pointer-implicit-conversion")]
    [Description("Checks for conditions based on implicit conversion from a bool pointer to bool. Example: ")]
    [ClangCheck(true)]
    public bool MiscBoolPointerImplicitConversion { get; set; }

    [Category("Checks")]
    [DisplayName("misc-dangling-handle")]
    [Description("Detect dangling references in value handles like std::experimental::string_view. These dangling references can be a result of constructing handles from temporary values, where the temporary is destroyed soon after the handle is created.  ")]
    [ClangCheck(true)]
    public bool MiscDanglingHandle { get; set; }

    [Category("Checks")]
    [DisplayName("misc-definitions-in-headers")]
    [Description("Finds non-extern non-inline function and variable definitions in header files, which can lead to potential ODR violations in case these headers are included from multiple translation units. ")]
    [ClangCheck(true)]
    public bool MiscDefinitionsInHeaders { get; set; }

    [Category("Checks")]
    [DisplayName("misc-fold-init-type")]
    [Description("The check flags type mismatches in folds like std::accumulate that might result in loss of precision. std::accumulate folds an input range into an initial value using the type of the latter, with operator+ by default. This can cause loss of precision through: ")]
    [ClangCheck(true)]
    public bool MiscFoldInitType { get; set; }

    [Category("Checks")]
    [DisplayName("misc-forward-declaration-namespace")]
    [Description("Checks if an unused forward declaration is in a wrong namespace. The check inspects all unused forward declarations and checks if there is any declaration/definition with the same name existing, which could indicate that the forward declaration is in a potentially wrong namespace. This check can only generate warnings, but it can’t suggest a fix at this point. ")]
    [ClangCheck(true)]
    public bool MiscForwardDeclarationNamespace { get; set; }

    [Category("Checks")]
    [DisplayName("misc-forwarding-reference-overload")]
    [Description("The check looks for perfect forwarding constructors that can hide copy or move constructors. If a non const lvalue reference is passed to the constructor, the forwarding reference parameter will be a better match than the const reference parameter of the copy constructor, so the perfect forwarding constructor will be called, which can be confusing. For detailed description of this issue see: Scott Meyers, Effective Modern C++, Item 26. Consider the following example: The check warns for constructors C1 and C2, because those can hide copy and move constructors. We suppress warnings if the copy and the move constructors are both disabled (deleted or private), because there is nothing the perfect forwarding constructor could hide in this case. We also suppress warnings for constructors like C3 that are guarded with an enable_if, assuming the programmer was aware of the possible hiding. ")]
    [ClangCheck(true)]
    public bool MiscForwardingReferenceOverload { get; set; }

    [Category("Checks")]
    [DisplayName("misc-inaccurate-erase")]
    [Description("Checks for inaccurate use of the erase() method. Algorithms like remove() do not actually remove any element from the container but return an iterator to the first redundant element at the end of the container. These redundant elements must be removed using the erase() method. This check warns when not all of the elements will be removed due to using an inappropriate overload. ")]
    [ClangCheck(true)]
    public bool MiscInaccurateErase { get; set; }

    [Category("Checks")]
    [DisplayName("misc-incorrect-roundings")]
    [Description("Checks the usage of patterns known to produce incorrect rounding. Programmers often use: to round the double expression to an integer. The problem with this: ")]
    [ClangCheck(true)]
    public bool MiscIncorrectRoundings { get; set; }

    [Category("Checks")]
    [DisplayName("misc-inefficient-algorithm")]
    [Description("Warns on inefficient use of STL algorithms on associative containers. Associative containers implements some of the algorithms as methods which should be preferred to the algorithms in the algorithm header. The methods can take advanatage of the order of the elements. ")]
    [ClangCheck(true)]
    public bool MiscInefficientAlgorithm { get; set; }

    [Category("Checks")]
    [DisplayName("misc-lambda-function-name")]
    [Description("Checks for attempts to get the name of a function from within a lambda expression. The name of a lambda is always something like operator(), which is almost never what was intended. Example: Output: Likely intended output: ")]
    [ClangCheck(true)]
    public bool MiscLambdaFunctionName { get; set; }

    [Category("Checks")]
    [DisplayName("misc-macro-parentheses")]
    [Description("Finds macros that can have unexpected behaviour due to missing parentheses. Macros are expanded by the preprocessor as-is. As a result, there can be unexpected behaviour; operators may be evaluated in unexpected order and unary operators may become binary operators, etc. When the replacement list has an expression, it is recommended to surround it with parentheses. This ensures that the macro result is evaluated completely before it is used. It is also recommended to surround macro arguments in the replacement list with parentheses. This ensures that the argument value is calculated properly. ")]
    [ClangCheck(true)]
    public bool MiscMacroParentheses { get; set; }

    [Category("Checks")]
    [DisplayName("misc-macro-repeated-side-effects")]
    [Description("Checks for repeated argument with side effects in macros. ")]
    [ClangCheck(true)]
    public bool MiscMacroRepeatedSideEffects { get; set; }

    [Category("Checks")]
    [DisplayName("misc-misplaced-const")]
    [Description("This check diagnoses when a const qualifier is applied to a typedef to a pointer type rather than to the pointee, because such constructs are often misleading to developers because the const applies to the pointer rather than the pointee. For instance, in the following code, the resulting type is int * const rather than const int *: The check does not diagnose when the underlying typedef type is a pointer to a const type or a function pointer type. This is because the const qualifier is less likely to be mistaken because it would be redundant (or disallowed) on the underlying pointee type. ")]
    [ClangCheck(true)]
    public bool MiscMisplacedConst { get; set; }

    [Category("Checks")]
    [DisplayName("misc-misplaced-widening-cast")]
    [Description("This check will warn when there is a cast of a calculation result to a bigger type. If the intention of the cast is to avoid loss of precision then the cast is misplaced, and there can be loss of precision. Otherwise the cast is ineffective. Example code: The result x * 1000 is first calculated using int precision. If the result exceeds int precision there is loss of precision. Then the result is casted to long. If there is no loss of precision then the cast can be removed or you can explicitly cast to int instead. If you want to avoid loss of precision then put the cast in a proper location, for instance: ")]
    [ClangCheck(true)]
    public bool MiscMisplacedWideningCast { get; set; }

    [Category("Checks")]
    [DisplayName("misc-move-const-arg")]
    [Description("The check warns In all three cases, the check will suggest a fix that removes the std::move(). Here are examples of each of the three cases: ")]
    [ClangCheck(true)]
    public bool MiscMoveConstArg { get; set; }

    [Category("Checks")]
    [DisplayName("misc-move-constructor-init")]
    [Description("“cert-oop11-cpp” redirects here as an alias for this check. The check flags user-defined move constructors that have a ctor-initializer initializing a member or base class through a copy constructor instead of a move constructor. ")]
    [ClangCheck(true)]
    public bool MiscMoveConstructorInit { get; set; }

    [Category("Checks")]
    [DisplayName("misc-move-forwarding-reference")]
    [Description("Warns if std::move is called on a forwarding reference, for example: Forwarding references should typically be passed to std::forward instead of std::move, and this is the fix that will be suggested. (A forwarding reference is an rvalue reference of a type that is a deduced function template argument.) In this example, the suggested fix would be ")]
    [ClangCheck(true)]
    public bool MiscMoveForwardingReference { get; set; }

    [Category("Checks")]
    [DisplayName("misc-multiple-statement-macro")]
    [Description("Detect multiple statement macros that are used in unbraced conditionals. Only the first statement of the macro will be inside the conditional and the other ones will be executed unconditionally. Example: ")]
    [ClangCheck(true)]
    public bool MiscMultipleStatementMacro { get; set; }

    [Category("Checks")]
    [DisplayName("misc-new-delete-overloads")]
    [Description("cert-dcl54-cpp redirects here as an alias for this check. The check flags overloaded operator new() and operator delete() functions that do not have a corresponding free store function defined within the same scope. For instance, the check will flag a class implementation of a non-placement operator new() when the class does not also define a non-placement operator delete() function as well. The check does not flag implicitly-defined operators, deleted or private operators, or placement operators. This check corresponds to CERT C++ Coding Standard rule DCL54-CPP. Overload allocation and deallocation functions as a pair in the same scope. ")]
    [ClangCheck(true)]
    public bool MiscNewDeleteOverloads { get; set; }

    [Category("Checks")]
    [DisplayName("misc-noexcept-move-constructor")]
    [Description("The check flags user-defined move constructors and assignment operators not marked with noexcept or marked with noexcept(expr) where expr evaluates to false (but is not a false literal itself). Move constructors of all the types used with STL containers, for example, need to be declared noexcept. Otherwise STL will choose copy constructors instead. The same is valid for move assignment operations. ")]
    [ClangCheck(true)]
    public bool MiscNoexceptMoveConstructor { get; set; }

    [Category("Checks")]
    [DisplayName("misc-non-copyable-objects")]
    [Description("cert-fio38-c redirects here as an alias for this check. The check flags dereferences and non-pointer declarations of objects that are not meant to be passed by value, such as C FILE objects or POSIX pthread_mutex_t objects. This check corresponds to CERT C++ Coding Standard rule FIO38-C. Do not copy a FILE object. ")]
    [ClangCheck(true)]
    public bool MiscNonCopyableObjects { get; set; }

    [Category("Checks")]
    [DisplayName("misc-redundant-expression")]
    [Description("Detect redundant expressions which are typically errors due to copy-paste. Depending on the operator expressions may be Example: ")]
    [ClangCheck(true)]
    public bool MiscRedundantExpression { get; set; }

    [Category("Checks")]
    [DisplayName("misc-sizeof-container")]
    [Description("The check finds usages of sizeof on expressions of STL container types. Most likely the user wanted to use .size() instead. All class/struct types declared in namespace std:: having a const size() method are considered containers, with the exception of std::bitset and std::array.  ")]
    [ClangCheck(true)]
    public bool MiscSizeofContainer { get; set; }

    [Category("Checks")]
    [DisplayName("misc-sizeof-expression")]
    [Description("The check finds usages of sizeof expressions which are most likely errors. The sizeof operator yields the size (in bytes) of its operand, which may be an expression or the parenthesized name of a type. Misuse of this operator may be leading to errors and possible software vulnerabilities. ")]
    [ClangCheck(true)]
    public bool MiscSizeofExpression { get; set; }

    [Category("Checks")]
    [DisplayName("misc-static-assert")]
    [Description("cert-dcl03-c redirects here as an alias for this check. Replaces assert() with static_assert() if the condition is evaluatable at compile time. The condition of static_assert() is evaluated at compile time which is safer and more efficient. ")]
    [ClangCheck(true)]
    public bool MiscStaticAssert { get; set; }

    [Category("Checks")]
    [DisplayName("misc-string-compare")]
    [Description("Finds string comparisons using the compare method. A common mistake is to use the string’s compare method instead of using the equality or inequality operators. The compare method is intended for sorting functions and thus returns a negative number, a positive number or zero depending on the lexicographical relationship between the strings compared. If an equality or inequality check can suffice, that is recommended. This is recommended to avoid the risk of incorrect interpretation of the return value and to simplify the code. The string equality and inequality operators can also be faster than the compare method due to early termination.  The above code examples shows the list of if-statements that this check will give a warning for. All of them uses compare to check if equality or inequality of two strings instead of using the correct operators. ")]
    [ClangCheck(true)]
    public bool MiscStringCompare { get; set; }

    [Category("Checks")]
    [DisplayName("misc-string-constructor")]
    [Description("Finds string constructors that are suspicious and probably errors. A common mistake is to swap parameters to the ‘fill’ string-constructor.  Calling the string-literal constructor with a length bigger than the literal is suspicious and adds extra random characters to the string.  Creating an empty string from constructors with parameters is considered suspicious. The programmer should use the empty constructor instead.  ")]
    [ClangCheck(true)]
    public bool MiscStringConstructor { get; set; }

    [Category("Checks")]
    [DisplayName("misc-string-integer-assignment")]
    [Description("The check finds assignments of an integer to std::basic_string<CharT> (std::string, std::wstring, etc.). The source of the problem is the following assignment operator of std::basic_string<CharT>: Numeric types can be implicitly casted to character types. Use the appropriate conversion functions or character literals. In order to suppress false positives, use an explicit cast. ")]
    [ClangCheck(true)]
    public bool MiscStringIntegerAssignment { get; set; }

    [Category("Checks")]
    [DisplayName("misc-string-literal-with-embedded-nul")]
    [Description("Finds occurrences of string literal with embedded NUL character and validates their usage. ")]
    [ClangCheck(true)]
    public bool MiscStringLiteralWithEmbeddedNul { get; set; }

    [Category("Checks")]
    [DisplayName("misc-suspicious-enum-usage")]
    [Description("The checker detects various cases when an enum is probably misused (as a bitmask ). The following cases will be investigated only using StrictMode. We regard the enum as a (suspicious) bitmask if the three conditions below are true at the same time: So whenever the non pow-of-2 element is used as a bitmask element we diagnose a misuse and give a warning.  ")]
    [ClangCheck(true)]
    public bool MiscSuspiciousEnumUsage { get; set; }

    [Category("Checks")]
    [DisplayName("misc-suspicious-missing-comma")]
    [Description("String literals placed side-by-side are concatenated at translation phase 6 (after the preprocessor). This feature is used to represent long string literal on multiple lines. For instance, the following declarations are equivalent: A common mistake done by programmers is to forget a comma between two string literals in an array initializer list. The array contains the string “line 2line3” at offset 1 (i.e. Test[1]). Clang won’t generate warnings at compile time. This check may warn incorrectly on cases like: ")]
    [ClangCheck(true)]
    public bool MiscSuspiciousMissingComma { get; set; }

    [Category("Checks")]
    [DisplayName("misc-suspicious-semicolon")]
    [Description("Finds most instances of stray semicolons that unexpectedly alter the meaning of the code. More specifically, it looks for if, while, for and for-range statements whose body is a single semicolon, and then analyzes the context of the code (e.g. indentation) in an attempt to determine whether that is intentional. Here the body of the if statement consists of only the semicolon at the end of the first line, and x will be incremented regardless of the condition. As a result of this code, processLine() will only be called once, when the while loop with the empty body exits with line == NULL. The indentation of the code indicates the intention of the programmer. While the indentation does not imply any nesting, there is simply no valid reason to have an if statement with an empty body (but it can make sense for a loop). So this check issues a warning for the code above. To solve the issue remove the stray semicolon or in case the empty body is intentional, reflect this using code indentation or put the semicolon in a new line. For example: Here the second line is indented in a way that suggests that it is meant to be the body of the while loop - whose body is in fact empty, because of the semicolon at the end of the first line. Either remove the indentation from the second line: ... or move the semicolon from the end of the first line to a new line: In this case the check will assume that you know what you are doing, and will not raise a warning. ")]
    [ClangCheck(true)]
    public bool MiscSuspiciousSemicolon { get; set; }

    [Category("Checks")]
    [DisplayName("misc-suspicious-string-compare")]
    [Description("Find suspicious usage of runtime string comparison functions. This check is valid in C and C++. Checks for calls with implicit comparator and proposed to explicitly add it. Checks that compare function results (i,e, strcmp) are compared to valid constant. The resulting value is A common mistake is to compare the result to 1 or -1. Additionally, the check warns if the results value is implicitly cast to a suspicious non-integer type. It’s happening when the returned value is used in a wrong context. ")]
    [ClangCheck(true)]
    public bool MiscSuspiciousStringCompare { get; set; }

    [Category("Checks")]
    [DisplayName("misc-swapped-arguments")]
    [Description("Finds potentially swapped arguments by looking at implicit conversions. ")]
    [ClangCheck(true)]
    public bool MiscSwappedArguments { get; set; }

    [Category("Checks")]
    [DisplayName("misc-throw-by-value-catch-by-reference")]
    [Description("“cert-err09-cpp” redirects here as an alias for this check. “cert-err61-cpp” redirects here as an alias for this check. Finds violations of the rule “Throw by value, catch by reference” presented for example in “C++ Coding Standards” by H. Sutter and A. Alexandrescu. ")]
    [ClangCheck(true)]
    public bool MiscThrowByValueCatchByReference { get; set; }

    [Category("Checks")]
    [DisplayName("misc-unconventional-assign-operator")]
    [Description("Finds declarations of assign operators with the wrong return and/or argument types and definitions with good return type but wrong return statements. ")]
    [ClangCheck(true)]
    public bool MiscUnconventionalAssignOperator { get; set; }

    [Category("Checks")]
    [DisplayName("misc-undelegated-constructor")]
    [Description("Finds creation of temporary objects in constructors that look like a function call to another constructor of the same class. The user most likely meant to use a delegating constructor or base class initializer. ")]
    [ClangCheck(true)]
    public bool MiscUndelegatedConstructor { get; set; }

    [Category("Checks")]
    [DisplayName("misc-uniqueptr-reset-release")]
    [Description("Find and replace unique_ptr::reset(release()) with std::move(). Example: If y is already rvalue, std::move() is not added. x and y can also be std::unique_ptr<Foo>*. ")]
    [ClangCheck(true)]
    public bool MiscUniqueptrResetRelease { get; set; }

    [Category("Checks")]
    [DisplayName("misc-unused-alias-decls")]
    [Description("Finds unused namespace alias declarations. ")]
    [ClangCheck(true)]
    public bool MiscUnusedAliasDecls { get; set; }

    [Category("Checks")]
    [DisplayName("misc-unused-parameters")]
    [Description("Finds unused parameters and fixes them, so that -Wunused-parameter can be turned on. ")]
    [ClangCheck(true)]
    public bool MiscUnusedParameters { get; set; }

    [Category("Checks")]
    [DisplayName("misc-unused-raii")]
    [Description("Finds temporaries that look like RAII objects. The canonical example for this is a scoped lock. The destructor of the scoped_lock is called before the critical_section is entered, leaving it unprotected. We apply a number of heuristics to reduce the false positive count of this check: ")]
    [ClangCheck(true)]
    public bool MiscUnusedRaii { get; set; }

    [Category("Checks")]
    [DisplayName("misc-unused-using-decls")]
    [Description("Finds unused using declarations. Example: ")]
    [ClangCheck(true)]
    public bool MiscUnusedUsingDecls { get; set; }

    [Category("Checks")]
    [DisplayName("misc-use-after-move")]
    [Description("Warns if an object is used after it has been moved, for example: The last line will trigger a warning that str is used after it has been moved. The check does not trigger a warning if the object is reinitialized after the move and before the use. For example, no warning will be output for this code: The check takes control flow into account. A warning is only emitted if the use can be reached from the move. This means that the following code does not produce a warning: On the other hand, the following code does produce a warning: (The use-after-move happens on the second iteration of the loop.) In some cases, the check may not be able to detect that two branches are mutually exclusive. For example (assuming that i is an int): In this case, the check will erroneously produce a warning, even though it is not possible for both the move and the use to be executed. An erroneous warning can be silenced by reinitializing the object after the move: Subsections below explain more precisely what exactly the check considers to be a move, use, and reinitialization. ")]
    [ClangCheck(true)]
    public bool MiscUseAfterMove { get; set; }

    [Category("Checks")]
    [DisplayName("misc-virtual-near-miss")]
    [Description("Warn if a function is a near miss (ie. the name is very similar and the function signiture is the same) to a virtual function from a base class. Example: ")]
    [ClangCheck(true)]
    public bool MiscVirtualNearMiss { get; set; }

    [Category("Checks")]
    [DisplayName("modernize-avoid-bind")]
    [Description("The check finds uses of std::bind and replaces simple uses with lambdas. Lambdas will use value-capture where required. Right now it only handles free functions, not member functions. Given: Then: is replaced by: std::bind can be hard to read and can result in larger object files and binaries due to type information that will not be produced by equivalent lambdas. ")]
    [ClangCheck(true)]
    public bool ModernizeAvoidBind { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-deprecated-headers")]
    [Description("Some headers from C library were deprecated in C++ and are no longer welcome in C++ codebases. Some have no effect in C++. For more details refer to the C++ 14 Standard [depr.c.headers] section. This check replaces C standard library headers with their C++ alternatives and removes redundant ones. Improtant note: the Standard doesn’t guarantee that the C++ headers declare all the same functions in the global namespace. The check in its current form can break the code that uses library symbols from the global namespace. If the specified standard is older than C++11 the check will only replace headers deprecated before C++11, otherwise – every header that appeared in the previous list. These headers don’t have effect in C++: ")]
    [ClangCheck(true)]
    public bool ModernizeDeprecatedHeaders { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-loop-convert")]
    [Description("This check converts for(...; ...; ...) loops to use the new range-based loops in C++11. Three kinds of loops can be converted: ")]
    [ClangCheck(true)]
    public bool ModernizeLoopConvert { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-make-shared")]
    [Description("This check finds the creation of std::shared_ptr objects by explicitly calling the constructor and a new expression, and replaces it with a call to std::make_shared. This check also finds calls to std::shared_ptr::reset() with a new expression, and replaces it with a call to std::make_shared. ")]
    [ClangCheck(true)]
    public bool ModernizeMakeShared { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-make-unique")]
    [Description("This check finds the creation of std::unique_ptr objects by explicitly calling the constructor and a new expression, and replaces it with a call to std::make_unique, introduced in C++14. This check also finds calls to std::unique_ptr::reset() with a new expression, and replaces it with a call to std::make_unique. ")]
    [ClangCheck(true)]
    public bool ModernizeMakeUnique { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-pass-by-value")]
    [Description("With move semantics added to the language and the standard library updated with move constructors added for many types it is now interesting to take an argument directly by value, instead of by const-reference, and then copy. This check allows the compiler to take care of choosing the best way to construct the copy. The transformation is usually beneficial when the calling code passes an rvalue and assumes the move construction is a cheap operation. This short example illustrates how the construction of the value happens: ")]
    [ClangCheck(true)]
    public bool ModernizePassByValue { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-raw-string-literal")]
    [Description("This check selectively replaces string literals containing escaped characters with raw string literals. Example: becomes The presence of any of the following escapes can cause the string to be converted to a raw string literal: \\, \', \", \\?, and octal or hexadecimal escapes for printable ASCII characters. A string literal containing only escaped newlines is a common way of writing lines of text output. Introducing physical newlines with raw string literals in this case is likely to impede readability. These string literals are left unchanged. An escaped horizontal tab, form feed, or vertical tab prevents the string literal from being converted. The presence of a horizontal tab, form feed or vertical tab in source code is not visually obvious. ")]
    [ClangCheck(true)]
    public bool ModernizeRawStringLiteral { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-redundant-void-arg")]
    [Description("Find and remove redundant void argument lists. ")]
    [ClangCheck(true)]
    public bool ModernizeRedundantVoidArg { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-replace-auto-ptr")]
    [Description("This check replaces the uses of the deprecated class std::auto_ptr by std::unique_ptr (introduced in C++11). The transfer of ownership, done by the copy-constructor and the assignment operator, is changed to match std::unique_ptr usage by using explicit calls to std::move(). Migration example: Since std::move() is a library function declared in <utility> it may be necessary to add this include. The check will add the include directive when necessary. ")]
    [ClangCheck(true)]
    public bool ModernizeReplaceAutoPtr { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-replace-random-shuffle")]
    [Description("This check will find occurrences of std::random_shuffle and replace it with std::shuffle. In C++17 std::random_shuffle will no longer be available and thus we need to replace it. Below are two examples of what kind of occurrences will be found and two examples of what it will be replaced with. Both of these examples will be replaced with: The second example will also receive a warning that randomFunc is no longer supported in the same way as before so if the user wants the same functionality, the user will need to change the implementation of the randomFunc. One thing to be aware of here is that std::random_device is quite expensive to initialize. So if you are using the code in a performance critical place, you probably want to initialize it elsewhere. ")]
    [ClangCheck(true)]
    public bool ModernizeReplaceRandomShuffle { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-return-braced-init-list")]
    [Description("Replaces explicit calls to the constructor in a return with a braced initializer list. This way the return type is not needlessly duplicated in the function definition and the return statement. ")]
    [ClangCheck(true)]
    public bool ModernizeReturnBracedInitList { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-shrink-to-fit")]
    [Description("Replace copy and swap tricks on shrinkable containers with the shrink_to_fit() method call. The shrink_to_fit() method is more readable and more effective than the copy and swap trick to reduce the capacity of a shrinkable container. Note that, the shrink_to_fit() method is only available in C++11 and up. ")]
    [ClangCheck(true)]
    public bool ModernizeShrinkToFit { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-unary-static-assert")]
    [Description("The check diagnoses any static_assert declaration with an empty string literal and provides a fix-it to replace the declaration with a single-argument static_assert declaration. The check is only applicable for C++17 and later code. The following code: is replaced by: ")]
    [ClangCheck(true)]
    public bool ModernizeUnaryStaticAssert { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-use-auto")]
    [Description("This check is responsible for using the auto type specifier for variable declarations to improve code readability and maintainability. For example: The auto type specifier will only be introduced in situations where the variable type matches the type of the initializer expression. In other words auto should deduce the same type that was originally spelled in the source. However, not every situation should be transformed: In this example using auto for builtins doesn’t improve readability. In other situations it makes the code less self-documenting impairing readability and maintainability. As a result, auto is used only introduced in specific situations described below. ")]
    [ClangCheck(true)]
    public bool ModernizeUseAuto { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-use-bool-literals")]
    [Description("Finds integer literals which are cast to bool. ")]
    [ClangCheck(true)]
    public bool ModernizeUseBoolLiterals { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-use-default-member-init")]
    [Description("This check converts a default constructor’s member initializers into the new default member initializers in C++11. Other member initializers that match the default member initializer are removed. This can reduce repeated code or allow use of ‘= default’. ")]
    [ClangCheck(true)]
    public bool ModernizeUseDefaultMemberInit { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-use-emplace")]
    [Description("The check flags insertions to an STL-style container done by calling the push_back method with an explicitly-constructed temporary of the container element type. In this case, the corresponding emplace_back method results in less verbose and potentially more efficient code. Right now the check doesn’t support push_front and insert. It also doesn’t support insert functions for associative containers because replacing insert with emplace may result in speed regression, but it might get support with some addition flag in the future. By default only std::vector, std::deque, std::list are considered. This list can be modified using the ContainersWithPushBack option. Before: After: By default, the check is able to remove unnecessary std::make_pair and std::make_tuple calls from push_back calls on containers of std::pair and std::tuple. Custom tuple-like types can be modified by the TupleTypes option; custom make functions can be modified by the TupleMakeFunctions option. The other situation is when we pass arguments that will be converted to a type inside a container. Before: After: In some cases the transformation would be valid, but the code wouldn’t be exception safe. In this case the calls of push_back won’t be replaced. This is because replacing it with emplace_back could cause a leak of this pointer if emplace_back would throw exception before emplacement (e.g. not enough memory to add a new element). For more info read item 42 - “Consider emplacement instead of insertion.” of Scott Meyers “Effective Modern C++”. The default smart pointers that are considered are std::unique_ptr, std::shared_ptr, std::auto_ptr. To specify other smart pointers or other classes use the SmartPointers option. Check also doesn’t fire if any argument of the constructor call would be: This check requires C++11 or higher to run. ")]
    [ClangCheck(true)]
    public bool ModernizeUseEmplace { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-use-equals-default")]
    [Description("This check replaces default bodies of special member functions with = default;. The explicitly defaulted function declarations enable more opportunities in optimization, because the compiler might treat explicitly defaulted functions as trivial. ")]
    [ClangCheck(true)]
    public bool ModernizeUseEqualsDefault { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-use-equals-delete")]
    [Description("This check marks unimplemented private special member functions with = delete. To avoid false-positives, this check only applies in a translation unit that has all other member functions implemented. ")]
    [ClangCheck(true)]
    public bool ModernizeUseEqualsDelete { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-use-noexcept")]
    [Description("This check replaces deprecated dynamic exception specifications with the appropriate noexcept specification (introduced in C++11).  By default this check will replace throw() with noexcept, and throw(<exception>[,...]) or throw(...) with noexcept(false). ")]
    [ClangCheck(true)]
    public bool ModernizeUseNoexcept { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-use-nullptr")]
    [Description("The check converts the usage of null pointer constants (eg. NULL, 0) to use the new C++11 nullptr keyword. ")]
    [ClangCheck(true)]
    public bool ModernizeUseNullptr { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-use-override")]
    [Description("Use C++11’s override and remove virtual where applicable. ")]
    [ClangCheck(true)]
    public bool ModernizeUseOverride { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-use-transparent-functors")]
    [Description("Prefer transparent functors to non-transparent ones. When using transparent functors, the type does not need to be repeated. The code is easier to read, maintain and less prone to errors. It is not possible to introduce unwanted conversions. It is not always a safe transformation though. The following case will be untouched to preserve the semantics. ")]
    [ClangCheck(true)]
    public bool ModernizeUseTransparentFunctors { get; set; } = true;

    [Category("Checks")]
    [DisplayName("modernize-use-using")]
    [Description("The check converts the usage of typedef with using keyword. Before: After: This check requires using C++11 or higher to run. ")]
    [ClangCheck(true)]
    public bool ModernizeUseUsing { get; set; } = true;

    [Category("Checks")]
    [DisplayName("mpi-buffer-deref")]
    [Description("This check verifies if a buffer passed to an MPI (Message Passing Interface) function is sufficiently dereferenced. Buffers should be passed as a single pointer or array. As MPI function signatures specify void * for their buffer types, insufficiently dereferenced buffers can be passed, like for example as double pointers or multidimensional arrays, without a compiler warning emitted.  ")]
    [ClangCheck(true)]
    public bool MpiBufferDeref { get; set; }

    [Category("Checks")]
    [DisplayName("mpi-type-mismatch")]
    [Description("This check verifies if buffer type and MPI (Message Passing Interface) datatype pairs match for used MPI functions. All MPI datatypes defined by the MPI standard (3.1) are verified by this check. User defined typedefs, custom MPI datatypes and null pointer constants are skipped, in the course of verification. Example: ")]
    [ClangCheck(true)]
    public bool MpiTypeMismatch { get; set; }

    [Category("Checks")]
    [DisplayName("performance-faster-string-find")]
    [Description("Optimize calls to std::string::find() and friends when the needle passed is a single character string literal. The character literal overload is more efficient.  ")]
    [ClangCheck(true)]
    public bool PerformanceFasterStringFind { get; set; }

    [Category("Checks")]
    [DisplayName("performance-for-range-copy")]
    [Description("Finds C++11 for ranges where the loop variable is copied in each iteration but it would suffice to obtain it by const reference. The check is only applied to loop variables of types that are expensive to copy which means they are not trivially copyable or have a non-trivial copy constructor or destructor. To ensure that it is safe to replace the copy with a const reference the following heuristic is employed: ")]
    [ClangCheck(true)]
    public bool PerformanceForRangeCopy { get; set; }

    [Category("Checks")]
    [DisplayName("performance-implicit-cast-in-loop")]
    [ClangCheck(true)]
    public bool PerformanceImplicitCastInLoop { get; set; }

    [Category("Checks")]
    [DisplayName("performance-inefficient-string-concatenation")]
    [Description("This check warns about the performance overhead arising from concatenating strings using the operator+, for instance: Instead of this structure you should use operator+= or std::string‘s (std::basic_string) class member function append(). For instance: Could be rewritten in a greatly more efficient way like: And this can be rewritten too: In a slightly more efficient way like: ")]
    [ClangCheck(true)]
    public bool PerformanceInefficientStringConcatenation { get; set; }

    [Category("Checks")]
    [DisplayName("performance-inefficient-vector-operation")]
    [Description("Finds possible inefficient std::vector operations (e.g. push_back, emplace_back) that may cause unnecessary memory reallocations. Currently, the check only detects following kinds of loops with a single statement body: ")]
    [ClangCheck(true)]
    public bool PerformanceInefficientVectorOperation { get; set; }

    [Category("Checks")]
    [DisplayName("performance-type-promotion-in-math-fn")]
    [Description("Finds calls to C math library functions (from math.h or, in C++, cmath) with implicit float to double promotions. For example, warns on ::sin(0.f), because this funciton’s parameter is a double. You probably meant to call std::sin(0.f) (in C++), or sinf(0.f) (in C). ")]
    [ClangCheck(true)]
    public bool PerformanceTypePromotionInMathFn { get; set; }

    [Category("Checks")]
    [DisplayName("performance-unnecessary-copy-initialization")]
    [Description("Finds local variable declarations that are initialized using the copy constructor of a non-trivially-copyable type but it would suffice to obtain a const reference. The check is only applied if it is safe to replace the copy by a const reference. This is the case when the variable is const qualified or when it is only used as a const, i.e. only const methods or operators are invoked on it, or it is used as const reference or value argument in constructors or function calls. Example: ")]
    [ClangCheck(true)]
    public bool PerformanceUnnecessaryCopyInitialization { get; set; }

    [Category("Checks")]
    [DisplayName("performance-unnecessary-value-param")]
    [Description("Flags value parameter declarations of expensive to copy types that are copied for each invocation but it would suffice to pass them by const reference. The check is only applied to parameters of types that are expensive to copy which means they are not trivially copyable or have a non-trivial copy constructor or destructor. To ensure that it is safe to replace the value parameter with a const reference the following heuristic is employed: Example: If the parameter is not const, only copied or assigned once and has a non-trivial move-constructor or move-assignment operator respectively the check will suggest to move it. Example: Will become: ")]
    [ClangCheck(true)]
    public bool PerformanceUnnecessaryValueParam { get; set; }

    [Category("Checks")]
    [DisplayName("readability-avoid-const-params-in-decls")]
    [Description("Checks whether a function declaration has parameters that are top level const. const values in declarations do not affect the signature of a function, so they should not be put there.  ")]
    [ClangCheck(true)]
    public bool ReadabilityAvoidConstParamsInDecls { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-braces-around-statements")]
    [Description("google-readability-braces-around-statements redirects here as an alias for this check. Checks that bodies of if statements and loops (for, do while, and while) are inside braces. Before: After: ")]
    [ClangCheck(true)]
    public bool ReadabilityBracesAroundStatements { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-container-size-empty")]
    [Description("Checks whether a call to the size() method can be replaced with a call to empty(). The emptiness of a container should be checked using the empty() method instead of the size() method. It is not guaranteed that size() is a constant-time function, and it is generally more efficient and also shows clearer intent to use empty(). Furthermore some containers may implement the empty() method but not implement the size() method. Using empty() whenever possible makes it easier to switch to another container in the future. The check issues warning if a container has size() and empty() methods matching following signatures: size_type can be any kind of integer type. ")]
    [ClangCheck(true)]
    public bool ReadabilityContainerSizeEmpty { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-deleted-default")]
    [Description("Checks that constructors and assignment operators marked as = default are not actually deleted by the compiler. ")]
    [ClangCheck(true)]
    public bool ReadabilityDeletedDefault { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-delete-null-pointer")]
    [Description("Checks the if statements where a pointer’s existence is checked and then deletes the pointer. The check is unnecessary as deleting a null pointer has no effect. ")]
    [ClangCheck(true)]
    public bool ReadabilityDeleteNullPointer { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-else-after-return")]
    [Description("LLVM Coding Standards advises to reduce indentation where possible and where it makes understanding code easier. Early exit is one of the suggested enforcements of that. Please do not use else or else if after something that interrupts control flow - like return, break, continue, throw. The following piece of code illustrates how the check works. This piece of code: Would be transformed into: This check helps to enforce this LLVM Coding Standards recommendation. ")]
    [ClangCheck(true)]
    public bool ReadabilityElseAfterReturn { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-function-size")]
    [Description("google-readability-function-size redirects here as an alias for this check. Checks for large functions based on various metrics. ")]
    [ClangCheck(true)]
    public bool ReadabilityFunctionSize { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-identifier-naming")]
    [Description("Checks for identifiers naming style mismatch. This check will try to enforce coding guidelines on the identifiers naming. It supports lower_case, UPPER_CASE, camelBack and CamelCase casing and tries to convert from one to another if a mismatch is detected. It also supports a fixed prefix and suffix that will be prepended or appended to the identifiers, regardless of the casing. Many configuration options are available, in order to be able to create different rules for different kind of identifier. In general, the rules are falling back to a more generic rule if the specific case is not configured. ")]
    [ClangCheck(true)]
    public bool ReadabilityIdentifierNaming { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-implicit-bool-cast")]
    [ClangCheck(true)]
    public bool ReadabilityImplicitBoolCast { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-inconsistent-declaration-parameter-name")]
    [Description("Find function declarations which differ in parameter names. Example: This check should help to enforce consistency in large projects, where it often happens that a definition of function is refactored, changing the parameter names, but its declaration in header file is not updated. With this check, we can easily find and correct such inconsistencies, keeping declaration and definition always in sync. Unnamed parameters are allowed and are not taken into account when comparing function declarations, for example: To help with refactoring, in some cases fix-it hints are generated to align parameter names to a single naming convention. This works with the assumption that the function definition is the most up-to-date version, as it directly references parameter names in its body. Example: In the case of multiple redeclarations or function template specializations, a warning is issued for every redeclaration or specialization inconsistent with the definition or the first declaration seen in a translation unit. ")]
    [ClangCheck(true)]
    public bool ReadabilityInconsistentDeclarationParameterName { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-misleading-indentation")]
    [Description("Correct indentation helps to understand code. Mismatch of the syntactical structure and the indentation of the code may hide serious problems. Missing braces can also make it significantly harder to read the code, therefore it is important to use braces. The way to avoid dangling else is to always check that an else belongs to the if that begins in the same column. You can omit braces when your inner part of e.g. an if statement has only one statement in it. Although in that case you should begin the next statement in the same column with the if.  ")]
    [ClangCheck(true)]
    public bool ReadabilityMisleadingIndentation { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-misplaced-array-index")]
    [Description("This check warns for unusual array index syntax. The following code has unusual array index syntax: becomes ")]
    [ClangCheck(true)]
    public bool ReadabilityMisplacedArrayIndex { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-named-parameter")]
    [Description("Find functions with unnamed arguments. The check implements the following rule originating in the Google C++ Style Guide: https://google.github.io/styleguide/cppguide.html#Function_Declarations_and_Definitions All parameters should be named, with identical names in the declaration and implementation. Corresponding cpplint.py check name: readability/function. ")]
    [ClangCheck(true)]
    public bool ReadabilityNamedParameter { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-non-const-parameter")]
    [Description("The check finds function parameters of a pointer type that could be changed to point to a constant type instead. When const is used properly, many mistakes can be avoided. Advantages when using const properly: This check is not strict about constness, it only warns when the constness will make the function interface safer. ")]
    [ClangCheck(true)]
    public bool ReadabilityNonConstParameter { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-redundant-control-flow")]
    [Description("This check looks for procedures (functions returning no value) with return statements at the end of the function. Such return statements are redundant. Loop statements (for, while, do while) are checked for redundant continue statements at the end of the loop body.  The following function f contains a redundant return statement: becomes The following function k contains a redundant continue statement: becomes ")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantControlFlow { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-redundant-declaration")]
    [Description("Finds redundant variable and function declarations. becomes Such redundant declarations can be removed without changing program behaviour. They can for instance be unintentional left overs from previous refactorings when code has been moved around. Having redundant declarations could in worst case mean that there are typos in the code that cause bugs. Normally the code can be automatically fixed, clang-tidy can remove the second declaration. However there are 2 cases when you need to fix the code manually: ")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantDeclaration { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-redundant-function-ptr-dereference")]
    [Description("Finds redundant dereferences of a function pointer. Before: After: ")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantFunctionPtrDereference { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-redundant-member-init")]
    [Description("Finds member initializations that are unnecessary because the same default constructor would be called if they were not present. Example: ")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantMemberInit { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-redundant-smartptr-get")]
    [Description("google-readability-redundant-smartptr-get redirects here as an alias for this check. Find and remove redundant calls to smart pointer’s .get() method.  ")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantSmartptrGet { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-redundant-string-cstr")]
    [Description("Finds unnecessary calls to std::string::c_str() and std::string::data(). ")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantStringCstr { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-redundant-string-init")]
    [Description("Finds unnecessary string initializations.  ")]
    [ClangCheck(true)]
    public bool ReadabilityRedundantStringInit { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-simplify-boolean-expr")]
    [Description("Looks for boolean expressions involving boolean constants and simplifies them to use the appropriate boolean expression directly.  ")]
    [ClangCheck(true)]
    public bool ReadabilitySimplifyBooleanExpr { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-static-definition-in-anonymous-namespace")]
    [Description("Finds static function and variable definitions in anonymous namespace. In this case, static is redundant, because anonymous namespace limits the visibility of definitions to a single translation unit. The check will apply a fix by removing the redundant static qualifier. ")]
    [ClangCheck(true)]
    public bool ReadabilityStaticDefinitionInAnonymousNamespace { get; set; } = true;

    [Category("Checks")]
    [DisplayName("readability-uniqueptr-delete-release")]
    [Description("Replace delete <unique_ptr>.release() with <unique_ptr> = nullptr. The latter is shorter, simpler and does not require use of raw pointer APIs. ")]
    [ClangCheck(true)]
    public bool ReadabilityUniqueptrDeleteRelease { get; set; } = true;


    protected override IWin32Window Window
    {
      get
      {
        ElementHost elementHost = new ElementHost();
        elementHost.Child = new ClangTidyPredefinedChecksOptionsUserControl(this);
        return elementHost;
      }
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

    #endregion

  }
}
