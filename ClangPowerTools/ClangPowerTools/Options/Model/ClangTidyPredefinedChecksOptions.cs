using System;
using System.ComponentModel;

namespace ClangPowerTools.Options.Model
{
  [Serializable]
  public class ClangTidyPredefinedChecksOptions
  {
    #region Properties

    public bool AndroidCloexecCreat { get; set; }

    public bool AndroidCloexecFopen { get; set; }

    public bool AndroidCloexecOpen { get; set; }

    public bool AndroidCloexecSocket { get; set; }

    public bool BoostUseToString { get; set; }

    public bool BugproneSuspiciousMemsetUsage { get; set; }

    public bool BugproneUndefinedMemoryManipulation { get; set; }

    public bool CertDcl03C { get; set; }

    public bool CertDcl21Cpp { get; set; }

    public bool CertDcl50Cpp { get; set; }

    public bool CertDcl54Cpp { get; set; }

    public bool CertDcl58Cpp { get; set; }

    public bool CertDcl59Cpp { get; set; }

    public bool CertEnv33C { get; set; }

    public bool CertErr09Cpp { get; set; }

    public bool CertErr34C { get; set; }

    public bool CertErr52Cpp { get; set; }

    public bool CertErr58Cpp { get; set; }

    public bool CertErr60Cpp { get; set; }

    public bool CertErr61Cpp { get; set; }

    public bool CertFio38C { get; set; }

    public bool CertFlp30C { get; set; }

    public bool CertMsc30C { get; set; }

    public bool CertMsc50Cpp { get; set; }

    public bool CertOop11Cpp { get; set; }

    public bool ClangAnalyzerApimodelinggoogleGTest { get; set; }

    public bool ClangAnalyzerCorebuiltinBuiltinFunctions { get; set; }

    public bool ClangAnalyzerCorebuiltinNoReturnFunctions { get; set; }

    public bool ClangAnalyzerCoreCallAndMessage { get; set; }

    public bool ClangAnalyzerCoreDivideZero { get; set; }

    public bool ClangAnalyzerCoreDynamicTypePropagation { get; set; }

    public bool ClangAnalyzerCoreNonNullParamChecker { get; set; }

    public bool ClangAnalyzerCoreNullDereference { get; set; }

    public bool ClangAnalyzerCoreStackAddressEscape { get; set; }

    public bool ClangAnalyzerCoreUndefinedBinaryOperatorResult { get; set; }

    public bool ClangAnalyzerCoreuninitializedArraySubscript { get; set; }

    public bool ClangAnalyzerCoreuninitializedAssign { get; set; }

    public bool ClangAnalyzerCoreuninitializedBranch { get; set; }

    public bool ClangAnalyzerCoreuninitializedCapturedBlockVariable { get; set; }

    public bool ClangAnalyzerCoreuninitializedUndefReturn { get; set; }

    public bool ClangAnalyzerCoreVLASize { get; set; }

    public bool ClangAnalyzerCplusplusNewDelete { get; set; }

    public bool ClangAnalyzerCplusplusNewDeleteLeaks { get; set; }

    public bool ClangAnalyzerCplusplusSelfAssignment { get; set; }

    public bool ClangAnalyzerDeadcodeDeadStores { get; set; }

    public bool ClangAnalyzerLlvmConventions { get; set; }

    public bool ClangAnalyzerNullabilityNullableDereferenced { get; set; }

    public bool ClangAnalyzerNullabilityNullablePassedToNonnull { get; set; }

    public bool ClangAnalyzerNullabilityNullableReturnedFromNonnull { get; set; }

    public bool ClangAnalyzerNullabilityNullPassedToNonnull { get; set; }

    public bool ClangAnalyzerNullabilityNullReturnedFromNonnull { get; set; }

    public bool ClangAnalyzerOptincplusplusVirtualCall { get; set; }

    public bool ClangAnalyzerOptinmpiMPIChecker { get; set; }

    public bool ClangAnalyzerOptinosxcocoalocalizabilityEmptyLocalizationContextChecker { get; set; }

    public bool ClangAnalyzerOptinosxcocoalocalizabilityNonLocalizedStringChecker { get; set; }

    public bool ClangAnalyzerOptinperformancePadding { get; set; }

    public bool ClangAnalyzerOptinportabilityUnixAPI { get; set; }

    public bool ClangAnalyzerOsxAPI { get; set; }

    public bool ClangAnalyzerOsxcocoaAtSync { get; set; }

    public bool ClangAnalyzerOsxcocoaClassRelease { get; set; }

    public bool ClangAnalyzerOsxcocoaDealloc { get; set; }

    public bool ClangAnalyzerOsxcocoaIncompatibleMethodTypes { get; set; }

    public bool ClangAnalyzerOsxcocoaLoops { get; set; }

    public bool ClangAnalyzerOsxcocoaMissingSuperCall { get; set; }

    public bool ClangAnalyzerOsxcocoaNilArg { get; set; }

    public bool ClangAnalyzerOsxcocoaNonNilReturnValue { get; set; }

    public bool ClangAnalyzerOsxcocoaNSAutoreleasePool { get; set; }

    public bool ClangAnalyzerOsxcocoaNSError { get; set; }

    public bool ClangAnalyzerOsxcocoaObjCGenerics { get; set; }

    public bool ClangAnalyzerOsxcocoaRetainCount { get; set; }

    public bool ClangAnalyzerOsxcocoaSelfInit { get; set; }

    public bool ClangAnalyzerOsxcocoaSuperDealloc { get; set; }

    public bool ClangAnalyzerOsxcocoaUnusedIvars { get; set; }

    public bool ClangAnalyzerOsxcocoaVariadicMethodTypes { get; set; }

    public bool ClangAnalyzerOsxcoreFoundationCFError { get; set; }

    public bool ClangAnalyzerOsxcoreFoundationCFNumber { get; set; }

    public bool ClangAnalyzerOsxcoreFoundationCFRetainRelease { get; set; }

    public bool ClangAnalyzerOsxcoreFoundationcontainersOutOfBounds { get; set; }

    public bool ClangAnalyzerOsxcoreFoundationcontainersPointerSizedValues { get; set; }

    public bool ClangAnalyzerOsxNumberObjectConversion { get; set; }

    public bool ClangAnalyzerOsxObjCProperty { get; set; }

    public bool ClangAnalyzerOsxSecKeychainAPI { get; set; }

    public bool ClangAnalyzerSecurityFloatLoopCounter { get; set; }

    public bool ClangAnalyzerSecurityinsecureAPIgetpw { get; set; }

    public bool ClangAnalyzerSecurityinsecureAPIgets { get; set; }

    public bool ClangAnalyzerSecurityinsecureAPImkstemp { get; set; }

    public bool ClangAnalyzerSecurityinsecureAPImktemp { get; set; }

    public bool ClangAnalyzerSecurityinsecureAPIrand { get; set; }

    public bool ClangAnalyzerSecurityinsecureAPIstrcpy { get; set; }

    public bool ClangAnalyzerSecurityinsecureAPIUncheckedReturn { get; set; }

    public bool ClangAnalyzerSecurityinsecureAPIvfork { get; set; }

    public bool ClangAnalyzerUnixAPI { get; set; }

    public bool ClangAnalyzerUnixcstringBadSizeArg { get; set; }

    public bool ClangAnalyzerUnixcstringNullArg { get; set; }

    public bool ClangAnalyzerUnixMalloc { get; set; }

    public bool ClangAnalyzerUnixMallocSizeof { get; set; }

    public bool ClangAnalyzerUnixMismatchedDeallocator { get; set; }

    public bool ClangAnalyzerUnixStdCLibraryFunctions { get; set; }

    public bool ClangAnalyzerUnixVfork { get; set; }

    public bool ClangAnalyzerValistCopyToSelf { get; set; }

    public bool ClangAnalyzerValistUninitialized { get; set; }

    public bool ClangAnalyzerValistUnterminated { get; set; }

    public bool CppcoreguidelinesCCopyAssignmentSignature { get; set; }

    public bool CppcoreguidelinesInterfacesGlobalInit { get; set; }

    public bool CppcoreguidelinesNoMalloc { get; set; }

    public bool CppcoreguidelinesProBoundsArrayToPointerDecay { get; set; }

    public bool CppcoreguidelinesProBoundsConstantArrayIndex { get; set; }

    public bool CppcoreguidelinesProBoundsPointerArithmetic { get; set; }

    public bool CppcoreguidelinesProTypeConstCast { get; set; }

    public bool CppcoreguidelinesProTypeCstyleCast { get; set; }

    public bool CppcoreguidelinesProTypeMemberInit { get; set; }

    public bool CppcoreguidelinesProTypeReinterpretCast { get; set; }

    public bool CppcoreguidelinesProTypeStaticCastDowncast { get; set; }

    public bool CppcoreguidelinesProTypeUnionAccess { get; set; }

    public bool CppcoreguidelinesProTypeVararg { get; set; }

    public bool CppcoreguidelinesSlicing { get; set; }

    public bool CppcoreguidelinesSpecialMemberFunctions { get; set; }

    public bool GoogleBuildExplicitMakePair { get; set; }

    public bool GoogleBuildNamespaces { get; set; }

    public bool GoogleBuildUsingNamespace { get; set; }

    public bool GoogleDefaultArguments { get; set; }

    public bool GoogleExplicitConstructor { get; set; }

    public bool GoogleGlobalNamesInHeaders { get; set; }

    public bool GoogleReadabilityBracesAroundStatements { get; set; } = true;

    public bool GoogleReadabilityCasting { get; set; } = true;

    public bool GoogleReadabilityFunctionSize { get; set; } = true;

    public bool GoogleReadabilityNamespaceComments { get; set; } = true;

    public bool GoogleReadabilityRedundantSmartptrGet { get; set; } = true;

    public bool GoogleReadabilityTodo { get; set; } = true;

    public bool GoogleRuntimeInt { get; set; }

    public bool GoogleRuntimeMemberStringReferences { get; set; }

    public bool GoogleRuntimeOperator { get; set; }

    public bool GoogleRuntimeReferences { get; set; }

    public bool HicppExplicitConversions { get; set; }

    public bool HicppFunctionSize { get; set; }

    public bool HicppInvalidAccessMoved { get; set; }

    public bool HicppMemberInit { get; set; }

    public bool HicppNamedParameter { get; set; }

    public bool HicppNewDeleteOperators { get; set; }

    public bool HicppNoAssembler { get; set; }

    public bool HicppNoexceptMove { get; set; }

    public bool HicppSpecialMemberFunctions { get; set; }

    public bool HicppUndelegatedConstructor { get; set; }

    public bool HicppUseEqualsDefault { get; set; }

    public bool HicppUseEqualsDelete { get; set; }

    public bool HicppUseOverride { get; set; }

    public bool LlvmHeaderGuard { get; set; }

    public bool LlvmIncludeOrder { get; set; }

    public bool LlvmNamespaceComment { get; set; }

    public bool LlvmTwineLocal { get; set; }

    public bool MiscArgumentComment { get; set; }

    public bool MiscAssertSideEffect { get; set; }

    public bool MiscBoolPointerImplicitConversion { get; set; }

    public bool MiscDanglingHandle { get; set; }

    public bool MiscDefinitionsInHeaders { get; set; }

    public bool MiscFoldInitType { get; set; }

    public bool MiscForwardDeclarationNamespace { get; set; }

    public bool MiscForwardingReferenceOverload { get; set; }

    public bool MiscInaccurateErase { get; set; }

    public bool MiscIncorrectRoundings { get; set; }

    public bool MiscInefficientAlgorithm { get; set; }

    public bool MiscLambdaFunctionName { get; set; }

    public bool MiscMacroParentheses { get; set; }

    public bool MiscMacroRepeatedSideEffects { get; set; }

    public bool MiscMisplacedConst { get; set; }

    public bool MiscMisplacedWideningCast { get; set; }

    public bool MiscMoveConstArg { get; set; }

    public bool MiscMoveConstructorInit { get; set; }

    public bool MiscMoveForwardingReference { get; set; }

    public bool MiscMultipleStatementMacro { get; set; }

    public bool MiscNewDeleteOverloads { get; set; }

    public bool MiscNoexceptMoveConstructor { get; set; }

    public bool MiscNonCopyableObjects { get; set; }

    public bool MiscRedundantExpression { get; set; }

    public bool MiscSizeofContainer { get; set; }

    public bool MiscSizeofExpression { get; set; }

    public bool MiscStaticAssert { get; set; }

    public bool MiscStringCompare { get; set; }

    public bool MiscStringConstructor { get; set; }

    public bool MiscStringIntegerAssignment { get; set; }

    public bool MiscStringLiteralWithEmbeddedNul { get; set; }

    public bool MiscSuspiciousEnumUsage { get; set; }

    public bool MiscSuspiciousMissingComma { get; set; }

    public bool MiscSuspiciousSemicolon { get; set; }

    public bool MiscSuspiciousStringCompare { get; set; }

    public bool MiscSwappedArguments { get; set; }

    public bool MiscThrowByValueCatchByReference { get; set; }

    public bool MiscUnconventionalAssignOperator { get; set; }

    public bool MiscUndelegatedConstructor { get; set; }

    public bool MiscUniqueptrResetRelease { get; set; }

    public bool MiscUnusedAliasDecls { get; set; }

    public bool MiscUnusedParameters { get; set; }

    public bool MiscUnusedRaii { get; set; }

    public bool MiscUnusedUsingDecls { get; set; }

    public bool MiscUseAfterMove { get; set; }

    public bool MiscVirtualNearMiss { get; set; }

    public bool ModernizeAvoidBind { get; set; } = true;

    public bool ModernizeDeprecatedHeaders { get; set; } = true;

    public bool ModernizeLoopConvert { get; set; } = true;

    public bool ModernizeMakeShared { get; set; } = true;

    public bool ModernizeMakeUnique { get; set; } = true;

    public bool ModernizePassByValue { get; set; } = true;

    public bool ModernizeRawStringLiteral { get; set; } = true;

    public bool ModernizeRedundantVoidArg { get; set; } = true;

    public bool ModernizeReplaceAutoPtr { get; set; } = true;

    public bool ModernizeReplaceRandomShuffle { get; set; } = true;

    public bool ModernizeReturnBracedInitList { get; set; } = true;

    public bool ModernizeShrinkToFit { get; set; } = true;

    public bool ModernizeUnaryStaticAssert { get; set; } = true;

    public bool ModernizeUseAuto { get; set; } = true;

    public bool ModernizeUseBoolLiterals { get; set; } = true;

    public bool ModernizeUseDefaultMemberInit { get; set; } = true;

    public bool ModernizeUseEmplace { get; set; } = true;

    public bool ModernizeUseEqualsDefault { get; set; } = true;

    public bool ModernizeUseEqualsDelete { get; set; } = true;

    public bool ModernizeUseNoexcept { get; set; } = true;

    public bool ModernizeUseNullptr { get; set; } = true;

    public bool ModernizeUseOverride { get; set; } = true;

    public bool ModernizeUseTransparentFunctors { get; set; } = true;

    public bool ModernizeUseUsing { get; set; } = true;

    public bool MpiBufferDeref { get; set; }

    public bool MpiTypeMismatch { get; set; }

    public bool PerformanceFasterStringFind { get; set; }

    public bool PerformanceForRangeCopy { get; set; }

    public bool PerformanceImplicitCastInLoop { get; set; }

    public bool PerformanceInefficientStringConcatenation { get; set; }

    public bool PerformanceInefficientVectorOperation { get; set; }

    public bool PerformanceTypePromotionInMathFn { get; set; }

    public bool PerformanceUnnecessaryCopyInitialization { get; set; }

    public bool PerformanceUnnecessaryValueParam { get; set; }

    public bool ReadabilityAvoidConstParamsInDecls { get; set; } = true;

    public bool ReadabilityBracesAroundStatements { get; set; } = true;

    public bool ReadabilityContainerSizeEmpty { get; set; } = true;

    public bool ReadabilityDeletedDefault { get; set; } = true;

    public bool ReadabilityDeleteNullPointer { get; set; } = true;

    public bool ReadabilityElseAfterReturn { get; set; } = true;

    public bool ReadabilityFunctionSize { get; set; } = true;

    public bool ReadabilityIdentifierNaming { get; set; } = true;

    public bool ReadabilityImplicitBoolCast { get; set; } = true;

    public bool ReadabilityInconsistentDeclarationParameterName { get; set; } = true;

    public bool ReadabilityMisleadingIndentation { get; set; } = true;

    public bool ReadabilityMisplacedArrayIndex { get; set; } = true;

    public bool ReadabilityNamedParameter { get; set; } = true;

    public bool ReadabilityNonConstParameter { get; set; } = true;

    public bool ReadabilityRedundantControlFlow { get; set; } = true;

    public bool ReadabilityRedundantDeclaration { get; set; } = true;

    public bool ReadabilityRedundantFunctionPtrDereference { get; set; } = true;

    public bool ReadabilityRedundantMemberInit { get; set; } = true;

    public bool ReadabilityRedundantSmartptrGet { get; set; } = true;

    public bool ReadabilityRedundantStringCstr { get; set; } = true;

    public bool ReadabilityRedundantStringInit { get; set; } = true;

    public bool ReadabilitySimplifyBooleanExpr { get; set; } = true;

    public bool ReadabilityStaticDefinitionInAnonymousNamespace { get; set; } = true;

    public bool ReadabilityUniqueptrDeleteRelease { get; set; } = true;

    #endregion

  }
}
