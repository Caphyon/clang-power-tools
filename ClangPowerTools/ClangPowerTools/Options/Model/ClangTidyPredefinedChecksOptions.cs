using System;

namespace ClangPowerTools.Options.Model
{
  [Serializable]
  public class ClangTidyPredefinedChecksOptions
  {
    #region Properties

    public bool AbseilDurationDivision { get; set; }

    public bool AbseilDurationFactoryFloat { get; set; }

    public bool AbseilDurationFactoryScale { get; set; }

    public bool AbseilFasterStrsplitDelimiter { get; set; }

    public bool AbseilNoInternalDependencies { get; set; }

    public bool AbseilNoNamespace { get; set; }

    public bool AbseilRedundantStrcatCalls { get; set; }

    public bool AbseilStrCatAppend { get; set; }

    public bool AbseilStringFindStartswith { get; set; }

    public bool AlphaosxcocoaMissingSuperCall { get; set; }

    public bool AndroidCloexecAccept { get; set; }

    public bool AndroidCloexecAccept4 { get; set; }

    public bool AndroidCloexecCreat { get; set; }

    public bool AndroidCloexecDup { get; set; }

    public bool AndroidCloexecEpollCreate { get; set; }

    public bool AndroidCloexecEpollCreate1 { get; set; }

    public bool AndroidCloexecFopen { get; set; }

    public bool AndroidCloexecInotifyInit { get; set; }

    public bool AndroidCloexecInotifyInit1 { get; set; }

    public bool AndroidCloexecMemfdCreate { get; set; }

    public bool AndroidCloexecOpen { get; set; }

    public bool AndroidCloexecSocket { get; set; }

    public bool AndroidComparisonInTempFailureRetry { get; set; }

    public bool BoostUseToString { get; set; }

    public bool BugproneArgumentComment { get; set; }

    public bool BugproneAssertSideEffect { get; set; }

    public bool BugproneBoolPointerImplicitConversion { get; set; }

    public bool BugproneCopyConstructorInit { get; set; }

    public bool BugproneDanglingHandle { get; set; }

    public bool BugproneExceptionEscape { get; set; }

    public bool BugproneFoldInitType { get; set; }

    public bool BugproneForwardDeclarationNamespace { get; set; }

    public bool BugproneForwardingReferenceOverload { get; set; }

    public bool BugproneInaccurateErase { get; set; }

    public bool BugproneIncorrectRoundings { get; set; }

    public bool BugproneIntegerDivision { get; set; }

    public bool BugproneLambdaFunctionName { get; set; }

    public bool BugproneMacroParentheses { get; set; }

    public bool BugproneMacroRepeatedSideEffects { get; set; }

    public bool BugproneMisplacedOperatorInStrlenInAlloc { get; set; }

    public bool BugproneMisplacedWideningCast { get; set; }

    public bool BugproneMoveForwardingReference { get; set; }

    public bool BugproneMultipleStatementMacro { get; set; }

    public bool BugproneParentVirtualCall { get; set; }

    public bool BugproneSizeofContainer { get; set; }

    public bool BugproneSizeofExpression { get; set; }

    public bool BugproneStringConstructor { get; set; }

    public bool BugproneStringIntegerAssignment { get; set; }

    public bool BugproneStringLiteralWithEmbeddedNul { get; set; }

    public bool BugproneSuspiciousEnumUsage { get; set; }

    public bool BugproneSuspiciousMemsetUsage { get; set; }

    public bool BugproneSuspiciousMissingComma { get; set; }

    public bool BugproneSuspiciousSemicolon { get; set; }

    public bool BugproneSuspiciousStringCompare { get; set; }

    public bool BugproneSwappedArguments { get; set; }

    public bool BugproneTerminatingContinue { get; set; }

    public bool BugproneThrowKeywordMissing { get; set; }

    public bool BugproneTooSmallLoopVariable { get; set; }

    public bool BugproneUndefinedMemoryManipulation { get; set; }

    public bool BugproneUndelegatedConstructor { get; set; }

    public bool BugproneUnusedRaii { get; set; }

    public bool BugproneUnusedReturnValue { get; set; }

    public bool BugproneUseAfterMove { get; set; }

    public bool BugproneVirtualNearMiss { get; set; }

    public bool CertDcl03C { get; set; }

    public bool CertDcl16C { get; set; }

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

    public bool CertMsc32C { get; set; }

    public bool CertMsc50Cpp { get; set; }

    public bool CertMsc51Cpp { get; set; }

    public bool CertOop11Cpp { get; set; }

    public bool CoreCallAndMessage { get; set; }

    public bool CoreDivideZero { get; set; }

    public bool CoreNonNullParamChecker { get; set; }

    public bool CoreNullDereference { get; set; }

    public bool CoreStackAddressEscape { get; set; }

    public bool CoreUndefinedBinaryOperatorResult { get; set; }

    public bool CoreuninitializedArraySubscript { get; set; }

    public bool CoreuninitializedAssign { get; set; }

    public bool CoreuninitializedBranch { get; set; }

    public bool CoreuninitializedCapturedBlockVariable { get; set; }

    public bool CoreuninitializedUndefReturn { get; set; }

    public bool CoreVLASize { get; set; }

    public bool CplusplusNewDelete { get; set; }

    public bool CplusplusNewDeleteLeaks { get; set; }

    public bool CppcoreguidelinesAvoidCArrays { get; set; }

    public bool CppcoreguidelinesAvoidGoto { get; set; }

    public bool CppcoreguidelinesAvoidMagicNumbers { get; set; }

    public bool CppcoreguidelinesCCopyAssignmentSignature { get; set; }

    public bool CppcoreguidelinesInterfacesGlobalInit { get; set; }

    public bool CppcoreguidelinesMacroUsage { get; set; }

    public bool CppcoreguidelinesNarrowingConversions { get; set; }

    public bool CppcoreguidelinesNoMalloc { get; set; }

    public bool CppcoreguidelinesNonPrivateMemberVariablesInClasses { get; set; }

    public bool CppcoreguidelinesOwningMemory { get; set; }

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

    public bool DeadcodeDeadStores { get; set; }

    public bool FuchsiaDefaultArguments { get; set; }

    public bool FuchsiaHeaderAnonNamespaces { get; set; }

    public bool FuchsiaMultipleInheritance { get; set; }

    public bool FuchsiaOverloadedOperator { get; set; }

    public bool FuchsiaRestrictSystemIncludes { get; set; }

    public bool FuchsiaStaticallyConstructedObjects { get; set; }

    public bool FuchsiaTrailingReturn { get; set; }

    public bool FuchsiaVirtualInheritance { get; set; }

    public bool GoogleBuildExplicitMakePair { get; set; }

    public bool GoogleBuildNamespaces { get; set; }

    public bool GoogleBuildUsingNamespace { get; set; }

    public bool GoogleDefaultArguments { get; set; }

    public bool GoogleExplicitConstructor { get; set; }

    public bool GoogleGlobalNamesInHeaders { get; set; }

    public bool GoogleObjcAvoidThrowingException { get; set; }

    public bool GoogleObjcFunctionNaming { get; set; }

    public bool GoogleObjcGlobalVariableDeclaration { get; set; }

    public bool GoogleReadabilityBracesAroundStatements { get; set; } = true;

    public bool GoogleReadabilityCasting { get; set; } = true;

    public bool GoogleReadabilityFunctionSize { get; set; } = true;

    public bool GoogleReadabilityNamespaceComments { get; set; } = true;

    public bool GoogleReadabilityTodo { get; set; } = true;

    public bool GoogleRuntimeInt { get; set; }

    public bool GoogleRuntimeOperator { get; set; }

    public bool GoogleRuntimeReferences { get; set; }

    public bool HicppAvoidCArrays { get; set; }

    public bool HicppAvoidGoto { get; set; }

    public bool HicppBracesAroundStatements { get; set; }

    public bool HicppDeprecatedHeaders { get; set; }

    public bool HicppExceptionBaseclass { get; set; }

    public bool HicppExplicitConversions { get; set; }

    public bool HicppFunctionSize { get; set; }

    public bool HicppInvalidAccessMoved { get; set; }

    public bool HicppMemberInit { get; set; }

    public bool HicppMoveConstArg { get; set; }

    public bool HicppMultiwayPathsCovered { get; set; }

    public bool HicppNamedParameter { get; set; }

    public bool HicppNewDeleteOperators { get; set; }

    public bool HicppNoArrayDecay { get; set; }

    public bool HicppNoAssembler { get; set; }

    public bool HicppNoexceptMove { get; set; }

    public bool HicppNoMalloc { get; set; }

    public bool HicppSignedBitwise { get; set; }

    public bool HicppSpecialMemberFunctions { get; set; }

    public bool HicppStaticAssert { get; set; }

    public bool HicppUndelegatedConstructor { get; set; }

    public bool HicppUppercaseLiteralSuffix { get; set; }

    public bool HicppUseAuto { get; set; }

    public bool HicppUseEmplace { get; set; }

    public bool HicppUseEqualsDefault { get; set; }

    public bool HicppUseEqualsDelete { get; set; }

    public bool HicppUseNoexcept { get; set; }

    public bool HicppUseNullptr { get; set; }

    public bool HicppUseOverride { get; set; }

    public bool HicppVararg { get; set; }

    public bool LlvmHeaderGuard { get; set; }

    public bool LlvmIncludeOrder { get; set; }

    public bool LlvmNamespaceComment { get; set; }

    public bool LlvmTwineLocal { get; set; }

    public bool MiscDefinitionsInHeaders { get; set; }

    public bool MiscMisplacedConst { get; set; }

    public bool MiscNewDeleteOverloads { get; set; }

    public bool MiscNonCopyableObjects { get; set; }

    public bool MiscNonPrivateMemberVariablesInClasses { get; set; }

    public bool MiscRedundantExpression { get; set; }

    public bool MiscStaticAssert { get; set; }

    public bool MiscThrowByValueCatchByReference { get; set; }

    public bool MiscUnconventionalAssignOperator { get; set; }

    public bool MiscUniqueptrResetRelease { get; set; }

    public bool MiscUnusedAliasDecls { get; set; }

    public bool MiscUnusedParameters { get; set; }

    public bool MiscUnusedUsingDecls { get; set; }

    public bool ModernizeAvoidBind { get; set; } = true;

    public bool ModernizeAvoidCArrays { get; set; } = true;

    public bool ModernizeConcatNestedNamespaces { get; set; } = true;

    public bool ModernizeDeprecatedHeaders { get; set; } = true;

    public bool ModernizeDeprecatedIosBaseAliases { get; set; } = true;

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

    public bool ModernizeUseUncaughtExceptions { get; set; } = true;

    public bool ModernizeUseUsing { get; set; } = true;

    public bool MpiBufferDeref { get; set; }

    public bool MpiTypeMismatch { get; set; }

    public bool NullabilityNullableDereferenced { get; set; }

    public bool NullabilityNullablePassedToNonnull { get; set; }

    public bool NullabilityNullPassedToNonnull { get; set; }

    public bool NullabilityNullReturnedFromNonnull { get; set; }

    public bool ObjcAvoidNserrorInit { get; set; }

    public bool ObjcAvoidSpinlock { get; set; }

    public bool ObjcForbiddenSubclassing { get; set; }

    public bool ObjcPropertyDeclaration { get; set; }

    public bool OptincplusplusVirtualCall { get; set; }

    public bool OptinmpiMPIChecker { get; set; }

    public bool OptinosxcocoalocalizabilityEmptyLocalizationContextChecker { get; set; }

    public bool OptinosxcocoalocalizabilityNonLocalizedStringChecker { get; set; }

    public bool OsxAPI { get; set; }

    public bool OsxcocoaAtSync { get; set; }

    public bool OsxcocoaClassRelease { get; set; }

    public bool OsxcocoaDealloc { get; set; }

    public bool OsxcocoaIncompatibleMethodTypes { get; set; }

    public bool OsxcocoaNilArg { get; set; }

    public bool OsxcocoaNSAutoreleasePool { get; set; }

    public bool OsxcocoaNSError { get; set; }

    public bool OsxcocoaObjCGenerics { get; set; }

    public bool OsxcocoaRetainCount { get; set; }

    public bool OsxcocoaSelfInit { get; set; }

    public bool OsxcocoaSuperDealloc { get; set; }

    public bool OsxcocoaUnusedIvars { get; set; }

    public bool OsxcocoaVariadicMethodTypes { get; set; }

    public bool OsxcoreFoundationCFError { get; set; }

    public bool OsxcoreFoundationCFNumber { get; set; }

    public bool OsxcoreFoundationCFRetainRelease { get; set; }

    public bool OsxcoreFoundationcontainersOutOfBounds { get; set; }

    public bool OsxcoreFoundationcontainersPointerSizedValues { get; set; }

    public bool OsxNumberObjectConversion { get; set; }

    public bool OsxSecKeychainAPI { get; set; }

    public bool PerformanceFasterStringFind { get; set; }

    public bool PerformanceForRangeCopy { get; set; }

    public bool PerformanceImplicitConversionInLoop { get; set; }

    public bool PerformanceInefficientAlgorithm { get; set; }

    public bool PerformanceInefficientStringConcatenation { get; set; }

    public bool PerformanceInefficientVectorOperation { get; set; }

    public bool PerformanceMoveConstArg { get; set; }

    public bool PerformanceMoveConstructorInit { get; set; }

    public bool PerformanceNoexceptMoveConstructor { get; set; }

    public bool PerformanceTypePromotionInMathFn { get; set; }

    public bool PerformanceUnnecessaryCopyInitialization { get; set; }

    public bool PerformanceUnnecessaryValueParam { get; set; }

    public bool PortabilitySimdIntrinsics { get; set; }

    public bool ReadabilityAvoidConstParamsInDecls { get; set; } = true;

    public bool ReadabilityBracesAroundStatements { get; set; } = true;

    public bool ReadabilityConstReturnType { get; set; } = true;

    public bool ReadabilityContainerSizeEmpty { get; set; } = true;

    public bool ReadabilityDeletedDefault { get; set; } = true;

    public bool ReadabilityDeleteNullPointer { get; set; } = true;

    public bool ReadabilityElseAfterReturn { get; set; } = true;

    public bool ReadabilityFunctionSize { get; set; } = true;

    public bool ReadabilityIdentifierNaming { get; set; } = true;

    public bool ReadabilityImplicitBoolConversion { get; set; } = true;

    public bool ReadabilityInconsistentDeclarationParameterName { get; set; } = true;

    public bool ReadabilityIsolateDeclaration { get; set; } = true;

    public bool ReadabilityMagicNumbers { get; set; } = true;

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

    public bool ReadabilitySimplifySubscriptExpr { get; set; } = true;

    public bool ReadabilityStaticAccessedThroughInstance { get; set; } = true;

    public bool ReadabilityStaticDefinitionInAnonymousNamespace { get; set; } = true;

    public bool ReadabilityStringCompare { get; set; } = true;

    public bool ReadabilityUniqueptrDeleteRelease { get; set; } = true;

    public bool ReadabilityUppercaseLiteralSuffix { get; set; } = true;

    public bool SecurityFloatLoopCounter { get; set; }

    public bool SecurityinsecureAPIbcmp { get; set; }

    public bool SecurityinsecureAPIbcopy { get; set; }

    public bool SecurityinsecureAPIbzero { get; set; }

    public bool SecurityinsecureAPIgetpw { get; set; }

    public bool SecurityinsecureAPIgets { get; set; }

    public bool SecurityinsecureAPImkstemp { get; set; }

    public bool SecurityinsecureAPImktemp { get; set; }

    public bool SecurityinsecureAPIrand { get; set; }

    public bool SecurityinsecureAPIstrcpy { get; set; }

    public bool SecurityinsecureAPIUncheckedReturn { get; set; }

    public bool SecurityinsecureAPIvfork { get; set; }

    public bool UnixAPI { get; set; }

    public bool UnixcstringBadSizeArg { get; set; }

    public bool UnixcstringNullArg { get; set; }

    public bool UnixMalloc { get; set; }

    public bool UnixMallocSizeof { get; set; }

    public bool UnixMismatchedDeallocator { get; set; }

    public bool UnixVfork { get; set; }

    public bool ZirconTemporaryObjects { get; set; }


    #endregion

  }
}
