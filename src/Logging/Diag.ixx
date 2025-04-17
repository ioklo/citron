export module Citron.Diag;

import "LoggingConfig.h";

import <variant>;
import <memory>;
import <vector>;

namespace Citron {

export struct Diag 
{
    virtual ~Diag() = default;
};
export struct Warn : Diag {};
export struct Error : Diag {};

export using DiagPtr = std::shared_ptr<Diag>;

export struct NestedDiag : Diag
{
    DiagPtr diag;
    DiagPtr child;
};

export struct AggregateDiag : Diag 
{
    std::vector<DiagPtr> diags;

    LOGGING_API AggregateDiag(std::vector<DiagPtr>&& diags) : diags(std::move(diags)) { }
};

//LOGGING_API void Fatal_Parameter_MismatchBetweenParamCountAndArgCount(); // A0401_Parameter_MismatchBetweenParamCountAndArgCount

//LOGGING_API void Fatal_IntTypeIsAllowedOnly(); // A0601_UnaryAssignOp_IntTypeIsAllowedOnly
//LOGGING_API void Fatal_UnaryAssignOp_AssignableExpressionIsAllowedOnly(); // A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly

//LOGGING_API void Fatal_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly(); // A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly
//LOGGING_API void Fatal_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly(); // A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly

//LOGGING_API void Fatal_BinaryOp_OperatorNotFound(); // A0802_BinaryOp_OperatorNotFound
//LOGGING_API void Fatal_BinaryOp_LeftOperandIsNotAssignable(); // A0803_BinaryOp_LeftOperandIsNotAssignable

//LOGGING_API void Fatal_CallExp_CallableExpressionIsNotCallable(); // A0902_CallExp_CallableExpressionIsNotCallable

//LOGGING_API void Fatal_IfStmt_ConditionShouldBeBool(); // A1001_IfStmt_ConditionShouldBeBool

//LOGGING_API void Fatal_ForStmt_ExpInitializerShouldBeAssignOrCall(); // A1102_ForStmt_ExpInitializerShouldBeAssignOrCall
//LOGGING_API void Fatal_ForStmt_ContinueExpShouldBeAssignOrCall(); // A1103_ForStmt_ContinueExpShouldBeAssignOrCall

//LOGGING_API void Fatal_ResolveIdentifier_TryAccessingPrivateMember(); // A2011_ResolveIdentifier_TryAccessingPrivateMember

//
//LOGGING_API void Fatal_ListExp_CantInferElementTypeWithEmptyElement(); // A1701_ListExp_CantInferElementTypeWithEmptyElement
//LOGGING_API void Fatal_ListExp_MismatchBetweenElementTypes();          // A1702_ListExp_MismatchBetweenElementTypes

//LOGGING_API void Fatal_StringExp_ExpElementShouldBeBoolOrIntOrString(); // A1901_StringExp_ExpElementShouldBeBoolOrIntOrString

//LOGGING_API void Fatal_ResolveIdentifier_VarWithTypeArg();                     // A2002_ResolveIdentifier_VarWithTypeArg
//LOGGING_API void Fatal_ResolveIdentifier_CantGetStaticMemberThroughInstance(); // A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance
//LOGGING_API void Fatal_ResolveIdentifier_CantGetTypeMemberThroughInstance();   // A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance
//LOGGING_API void Fatal_ResolveIdentifier_CantGetInstanceMemberThroughType();   // A2005_ResolveIdentifier_CantGetInstanceMemberThroughType
//LOGGING_API void Fatal_ResolveIdentifier_NotFound();                           // A2007_ResolveIdentifier_NotFound
//LOGGING_API void Fatal_ResolveIdentifier_FuncCantHaveMember();                 // A2006_ResolveIdentifier_FuncCantHaveMember
//LOGGING_API void Fatal_ResolveIdentifier_CantUseTypeAsExpression();            // A2008_ResolveIdentifier_CantUseTypeAsExpression
//LOGGING_API void Fatal_ResolveIdentifier_EnumElemCantHaveMember();             // A2009_ResolveIdentifier_EnumElemCantHaveMember
//LOGGING_API void Fatal_ResolveIdentifier_EnumInstanceCantHaveMember();         // A2013_ResolveIdentifier_EnumInstanceCantHaveMember // 2013겹침
//LOGGING_API void Fatal_ResolveIdentifier_CantUseNamespaceAsExpression();       // A2013_ResolveIdentifier_CantUseNamespaceAsExpression
//LOGGING_API void Fatal_ResolveIdentifier_ExpressionIsNotLocation();            // A2015_ResolveIdentifier_ExpressionIsNotLocation    
//LOGGING_API void Fatal_ResolveIdentifier_LambdaInstanceCantHaveMember();       // A2016_ResolveIdentifier_LambdaInstanceCantHaveMember
//LOGGING_API void Fatal_ResolveIdentifier_LocalPtrCantHaveMember();             // A2017_ResolveIdentifier_LocalPtrCantHaveMember
//LOGGING_API void Fatal_ResolveIdentifier_BoxPtrCantHaveMember();               // A2018_ResolveIdentifier_BoxPtrCantHaveMember
//LOGGING_API void Fatal_ResolveIdentifier_FuncInstanceCantHaveMember();         // A2019_ResolveIdentifier_FuncInstanceCantHaveMember

//LOGGING_API void Fatal_Cast_Failed();                   // A2201_Cast_Failed

//LOGGING_API void Fatal_NewExp_TypeIsNotClass();               // A2601_NewExp_TypeIsNotClass
//LOGGING_API void Fatal_NullLiteralExp_CantInferNullableType();        // A2701_NullLiteralExp_CantInferNullableType

//// IrExp -> NExp Translation
//LOGGING_API void Fatal_Reference_CantMakeReference();                 // A3001_Reference_CantMakeReference
//LOGGING_API void Fatal_Reference_CantReferenceTempValue();            // A3002_Reference_CantReferenceTempValue
//LOGGING_API void Fatal_Reference_UselessDereferenceReferencedValue(); // A3003_Reference_UselessDereferenceReferencedValue
//LOGGING_API void Fatal_Reference_CantReferenceThis();                 // A3004_Reference_CantReferenceThis

// Analyzer
// LOGGING_API void Fatal_VarDecl_CantUseEnumElementAsDeclType(); // A0105_VarDecl_CantUseEnumElementAsDeclType
// LOGGING_API void Fatal_VarDecl_RefInitializerUsedOnNonRefVarDecl(); // A0110_VarDecl_RefInitializerUsedOnNonRefVarDecl

// LOGGING_API void Fatal_MemberExp_InstanceTypeIsNotNormalType(); // A0301_MemberExp_InstanceTypeIsNotNormalType
// LOGGING_API void Fatal_MemberExp_TypeArgsForMemberVariableIsNotAllowed(); // A0302_MemberExp_TypeArgsForMemberVariableIsNotAllowed
// LOGGING_API void Fatal_MemberExp_MemberNotFound(); // A0303_MemberExp_MemberNotFound
// LOGGING_API void Fatal_MemberExp_MemberVariableIsNotStatic(); // A0304_MemberExp_MemberVariableIsNotStatic
// LOGGING_API void Fatal_MemberExp_MemberVariableIsStatic(); // A0305_MemberExp_MemberVariableIsStatic
// LOGGING_API void Fatal_MemberExp_MultipleCandidates(); // A0306_MemberExp_MultipleCandidates
// LOGGING_API void Fatal_MemberExp_MemberIsNotExpression(); // A0307_MemberExp_MemberIsNotExpression

// funcMatcher 자체에서 에러를 내지 않고, 각각 노드 처리기에서 에러를 생성하도록 한다
// LOGGING_API void Fatal_Parameter_MismatchBetweenParamTypeAndArgType(); // A0402_Parameter_MismatchBetweenParamTypeAndArgType => FuncMatcher..로 변경
// LOGGING_API void Fatal_IdExp_VariableNotFound(); // A0501_IdExp_VariableNotFound
// LOGGING_API void Fatal_IdExp_CantUseTypeAsExpression(); // A0502_IdExp_CantUseTypeAsExpression
// LOGGING_API void Fatal_IdExp_MultipleCandidates(); // A0503_IdExp_MultipleCandidates
// LOGGING_API void Fatal_BinaryOp_LeftOperandTypeIsNotCompatibleWithRightOperandType(); // A0801_BinaryOp_LeftOperandTypeIsNotCompatibleWithRightOperandType => CastFailed로 변경
// LOGGING_API void Fatal_IfStmt_TestTargetShouldBeLocalVariable(); // A1001_IfStmt_TestTargetShouldBeLocalVariable
// LOGGING_API void Fatal_IfStmt_TestTargetIdentifierNotFound(); // A1002_IfStmt_TestTargetIdentifierNotFound
// LOGGING_API void Fatal_IfStmt_TestTypeShouldBeEnumOrClass(); // A1003_IfStmt_TestTypeShouldBeEnumOrClass

// LOGGING_API void Fatal_YieldStmt_MismatchBetweenYieldValueAndSeqFuncYieldType(); // A1402_YieldStmt_MismatchBetweenYieldValueAndSeqFuncYieldType => CastFailed
// LOGGING_API void Fatal_ResolveIdentifier_TypeVarCantHaveMember(); // A2012_ResolveIdentifier_TypeVarCantHaveMember -> interface를 지키면 멤버가 있을 수 있다
//LOGGING_API void Fatal_FuncMatcher_MultipleCandidates(); // A2101_FuncMatcher_MultipleCandidates 
//LOGGING_API void Fatal_FuncMatcher_NotFound(); // A2102_FuncMatcher_NotFound

// LOGGING_API void Fatal_Identifier_MultipleCandidatesForIdentifier(); // A2001_Identifier_MultipleCandidatesForIdentifier
// A2901_BodyShouldReturn, // 리턴 타입이 있는 본문에 리턴이 없다

export struct Error_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType : Error { }; // A0102_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType
export struct Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope : Error { }; // A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope
export struct Error_VarDecl_GlobalVariableNameShouldBeUnique : Error { }; // A0104_VarDecl_GlobalVariableNameShouldBeUnique
export struct Error_VarDecl_RefDeclNeedInitializer : Error { }; // A0106_VarDecl_RefDeclNeedInitializer
export struct Error_VarDecl_LocalVarDeclNeedInitializer : Error { }; // A0111_VarDecl_LocalVarDeclNeedInitializer
export struct Error_VarDecl_RefDeclNeedLocationInitializer : Error { }; // A0112_VarDecl_RefDeclNeedLocationInitializer
export struct Error_VarDecl_UsingLocalVarInsteadOfVarWhenInitExpIsLocalInterface : Error { }; // A0113_VarDecl_UsingLocalVarInsteadOfVarWhenInitExpIsLocalInterface
export struct Error_VarDecl_UsingBoxPtrVarInsteadOfVarWhenInitExpIsBoxPtr : Error { }; // A0114_VarDecl_UsingBoxPtrVarInsteadOfVarWhenInitExpIsBoxPtr
export struct Error_VarDecl_UsingLocalPtrVarInsteadOfVarWhenInitExpIsLocalPtr : Error { }; // A0115_VarDecl_UsingLocalPtrVarInsteadOfVarWhenInitExpIsLocalPtr
export struct Error_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr : Error { }; // A0116_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr
export struct Error_VarDecl_UsingLocalVarAsDeclTypeButInitExpIsNotLocalInterface : Error { }; // A0117_VarDecl_UsingLocalVarAsDeclTypeButInitExpIsNotLocalInterface
export struct Error_VarDecl_UsingBoxPtrVarAsDeclTypeButInitExpIsNotBoxPtr : Error { }; // A0118_VarDecl_UsingBoxPtrVarAsDeclTypeButInitExpIsNotBoxPtr
export struct Error_VarDecl_UsingLocalPtrVarAsDeclTypeButInitExpIsNotLocalPtr : Error { }; // A0119_VarDecl_UsingLocalPtrVarAsDeclTypeButInitExpIsNotLocalPtr
export struct Error_VarDecl_UsingNullableVarAsDeclTypeButInitExpIsNotNullable : Error { }; // A0120_VarDecl_UsingNullableVarAsDeclTypeButInitExpIsNotNullable
export struct Error_VarDecl_InitExpTypeMismatch : Error { }; // A0121_VarDecl_InitExpTypeMismatch
export struct Error_Capturer_ReferencingLocalVariableIsNotAllowed : Error { }; // A0201_Capturer_ReferencingLocalVariableIsNotAllowed
export struct Error_Parameter_MismatchBetweenParamCountAndArgCount : Error { }; // A0401_Parameter_MismatchBetweenParamCountAndArgCount
export struct Error_Parameter_ParamsArgumentShouldBeTuple : Error { }; // A0402_Parameter_ParamsArgumentShouldBeTuple
export struct Error_UnaryAssignOp_IntTypeIsAllowedOnly : Error { }; // A0601_UnaryAssignOp_IntTypeIsAllowedOnly
export struct Error_UnaryAssignOp_AssignableExpressionIsAllowedOnly : Error { }; // A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly
export struct Error_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly : Error { }; // A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly
export struct Error_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly : Error { }; // A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly
export struct Error_BinaryOp_OperatorNotFound : Error { }; // A0802_BinaryOp_OperatorNotFound
export struct Error_BinaryOp_LeftOperandIsNotAssignable : Error { }; // A0803_BinaryOp_LeftOperandIsNotAssignable
export struct Error_CallExp_MultipleCandidates : Error { }; // A0901_CallExp_MultipleCandidates
export struct Error_CallExp_CallableExpressionIsNotCallable : Error { }; // A0902_CallExp_CallableExpressionIsNotCallable
export struct Error_CallExp_MismatchEnumCtorArgCount : Error { }; // A0903_CallExp_MismatchEnumCtorArgCount
export struct Error_CallExp_MismatchBetweenEnumParamTypeAndEnumArgType : Error { }; // A0904_CallExp_MismatchBetweenEnumParamTypeAndEnumArgType
export struct Error_CallExp_NoMatchedStructCtorFound : Error { }; // A0905_CallExp_NoMatchedStructCtorFound // TODO: A2602_NewExp_NoCtorFound 랑 겹침
export struct Error_CallExp_NotFound : Error { }; // A0906_CallExp_NotFound
export struct Error_CallExp_MultipleMatchedStructCtors : Error { }; // A0907_CallExp_MultipleMatchedStructCtors
export struct Error_CallExp_InstanceIsNotLocation : Error { }; // A0908_CallExp_InstanceIsNotLocation
export struct Error_IfStmt_ConditionShouldBeBool : Error { }; // A1001_IfStmt_ConditionShouldBeBool
export struct Error_IfTestStmt_CantDowncast : Error { }; // A2301_IfTestStmt_CantDowncast
export struct Error_ForStmt_ConditionShouldBeBool : Error { }; // A1101_ForStmt_ConditionShouldBeBool
export struct Error_ForStmt_ExpInitializerShouldBeAssignOrCall : Error { }; // A1102_ForStmt_ExpInitializerShouldBeAssignOrCall
export struct Error_ForStmt_ContinueExpShouldBeAssignOrCall : Error { }; // A1103_ForStmt_ContinueExpShouldBeAssignOrCall
export struct Error_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType : Error { }; // A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType
export struct Error_ReturnStmt_SeqFuncShouldReturnVoid : Error { }; // A1202_ReturnStmt_SeqFuncShouldReturnVoid
export struct Error_ReturnStmt_RefTargetIsNotLocation : Error { }; // A1203_ReturnStmt_RefTargetIsNotLocation
export struct Error_ExpStmt_ExpressionShouldBeAssignOrCall : Error { }; // A1301_ExpStmt_ExpressionShouldBeAssignOrCall
export struct Error_YieldStmt_YieldShouldBeInSeqFunc : Error { }; // A1401_YieldStmt_YieldShouldBeInSeqFunc
export struct Error_ContinueStmt_ShouldUsedInLoop : Error { }; // A1501_ContinueStmt_ShouldUsedInLoop
export struct Error_BreakStmt_ShouldUsedInLoop : Error { }; // A1601_BreakStmt_ShouldUsedInLoop
export struct Error_ListExp_CantInferElementTypeWithEmptyElement : Error { }; // A1701_ListExp_CantInferElementTypeWithEmptyElement
export struct Error_ListExp_MismatchBetweenElementTypes : Error { }; // A1702_ListExp_MismatchBetweenElementTypes
export struct Error_ForeachStmt_IteratorShouldBeListOrEnumerable : Error { }; // A1801_ForeachStmt_IteratorShouldBeListOrEnumerable // TODO: 추후에 더 일반적으로 바뀌어야 한다
export struct Error_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType : Error { }; // A1802_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType
export struct Error_StringExp_ExpElementShouldBeBoolOrIntOrString : Error { }; // A1901_StringExp_ExpElementShouldBeBoolOrIntOrString // TODO: 보다 일반적으로 바뀌어야 한다. ToString을 구현한 애들이 가능
export struct Error_ResolveIdentifier_MultipleCandidatesForIdentifier : Error { }; // A2001_ResolveIdentifier_MultipleCandidatesForIdentifier
export struct Error_ResolveIdentifier_VarWithTypeArg : Error { }; // A2002_ResolveIdentifier_VarWithTypeArg
export struct Error_ResolveIdentifier_CantGetStaticMemberThroughInstance : Error { }; // A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance // instance에서 static 함수, 변수를 가지고 오려고 했을 때, e.F, e.v (F, v가 static인 경우)
export struct Error_ResolveIdentifier_CantGetTypeMemberThroughInstance : Error { }; // A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance   // instance에서 타입을 가지고 오려고 했을 때, e.T
export struct Error_ResolveIdentifier_CantGetInstanceMemberThroughType : Error { }; // A2005_ResolveIdentifier_CantGetInstanceMemberThroughType
export struct Error_ResolveIdentifier_FuncCantHaveMember : Error { }; // A2006_ResolveIdentifier_FuncCantHaveMember
export struct Error_ResolveIdentifier_NotFound : Error { }; // A2007_ResolveIdentifier_NotFound
export struct Error_ResolveIdentifier_CantUseTypeAsExpression : Error { }; // A2008_ResolveIdentifier_CantUseTypeAsExpression // Type으로 Resolve는 되지만, 값으로 변경하려고 시도하다가 에러 var x = X.Y;
export struct Error_ResolveIdentifier_EnumElemCantHaveMember : Error { }; // A2009_ResolveIdentifier_EnumElemCantHaveMember
export struct Error_ResolveIdentifier_ThisIsNotInTheContext : Error { }; // A2010_ResolveIdentifier_ThisIsNotInTheContext   // 
export struct Error_ResolveIdentifier_TryAccessingPrivateMember : Error { }; // A2011_ResolveIdentifier_TryAccessingPrivateMember
export struct Error_ResolveIdentifier_EnumInstanceCantHaveMember : Error { }; // A2013_ResolveIdentifier_EnumInstanceCantHaveMember
export struct Error_ResolveIdentifier_CantUseNamespaceAsExpression : Error { }; // A2013_ResolveIdentifier_CantUseNamespaceAsExpression // 네임스페이스를 exp로 쓸 수 없습니다        
export struct Error_ResolveIdentifier_MultipleCandidatesForMember : Error { }; // A2014_ResolveIdentifier_MultipleCandidatesForMember
export struct Error_ResolveIdentifier_ExpressionIsNotLocation : Error { }; // A2015_ResolveIdentifier_ExpressionIsNotLocation
export struct Error_ResolveIdentifier_LambdaInstanceCantHaveMember : Error { }; // A2016_ResolveIdentifier_LambdaInstanceCantHaveMember
export struct Error_ResolveIdentifier_LocalPtrCantHaveMember : Error { }; // A2017_ResolveIdentifier_LocalPtrCantHaveMember
export struct Error_ResolveIdentifier_BoxPtrCantHaveMember : Error { }; // A2018_ResolveIdentifier_BoxPtrCantHaveMember
export struct Error_ResolveIdentifier_FuncInstanceCantHaveMember : Error { }; // A2019_ResolveIdentifier_FuncInstanceCantHaveMember
export struct Error_Cast_Failed : Error { }; // A2201_Cast_Failed
export struct Error_RootDecl_CannotSetPrivateAccessExplicitlyBecauseItsDefault : Error { }; // A2301_RootDecl_CannotSetPrivateAccessExplicitlyBecauseItsDefault
export struct Error_StructDecl_CannotSetMemberPublicAccessExplicitlyBecauseItsDefault : Error { }; // A2401_StructDecl_CannotSetMemberPublicAccessExplicitlyBecauseItsDefault
export struct Error_StructDecl_CannotSetMemberProtectedAccessBecauseItsNotAllowed : Error { }; // A2402_StructDecl_CannotSetMemberProtectedAccessBecauseItsNotAllowed
export struct Error_StructDecl_CannotDeclCtorDifferentWithTypeName : Error { }; // A2403_StructDecl_CannotDeclCtorDifferentWithTypeName
export struct Error_ClassDecl_CannotSetMemberPrivateAccessExplicitlyBecauseItsDefault : Error { }; // A2501_ClassDecl_CannotSetMemberPrivateAccessExplicitlyBecauseItsDefault
export struct Error_ClassDecl_CannotDeclCtorDifferentWithTypeName : Error { }; // A2502_ClassDecl_CannotDeclCtorDifferentWithTypeName
export struct Error_ClassDecl_CannotFindBaseClassCtor : Error { }; // A2503_ClassDecl_CannotFindBaseClassCtor
export struct Error_ClassDecl_CannotAccessBaseClassCtor : Error { }; // A2504_ClassDecl_CannotAccessBaseClassCtor
export struct Error_ClassDecl_TryCallBaseCtorWithoutBaseClass : Error { }; // A2505_ClassDecl_TryCallBaseCtorWithoutBaseClass
export struct Error_ClassDecl_CannotDecideWhichBaseCtorUse : Error { }; // A2506_ClassDecl_CannotDecideWhichBaseCtorUse
export struct Error_NewExp_TypeIsNotClass : Error { }; // A2601_NewExp_TypeIsNotClass
export struct Error_NewExp_NoMatchedClassCtor : Error { }; // A2602_NewExp_NoMatchedClassCtor // TODO: LOGGING_API void Fatal_CallExp_NoConstructorFound(); // A0905_CallExp_NoConstructorFound 랑 겹침
export struct Error_NewExp_MultipleMatchedClassCtors : Error { }; // A2603_NewExp_MultipleMatchedClassCtors // TODO: LOGGING_API void Fatal_CallExp_NoCtorFound(); // A0905_CallExp_NoConstructorFound 랑 겹침
export struct Error_NullLiteralExp_CantInferNullableType : Error { }; // A2701_NullLiteralExp_CantInferNullableType // null은 힌트와 쓰인다
export struct Error_StaticNotNullDirective_ShouldHaveOneArgument : Error { }; // A2801_StaticNotNullDirective_ShouldHaveOneArgument
export struct Error_StaticNotNullDirective_ArgumentMustBeLocation : Error { }; // A2802_StaticNotNullDirective_ArgumentMustBeLocation
export struct Error_Reference_CantMakeReference : Error { }; // A3001_Reference_CantMakeReference
export struct Error_Reference_CantReferenceTempValue : Error { }; // A3002_Reference_CantReferenceTempValue
export struct Error_Reference_UselessDereferenceReferencedValue : Error { }; // A3003_Reference_UselessDereferenceReferencedValue
export struct Error_Reference_CantReferenceThis : Error { }; // A3004_Reference_CantReferenceThis
export struct Error_NotSupported_LambdaParameterInference : Error { }; // A9901_NotSupported_LambdaParameterInference
export struct Error_NotSupported_LambdaReturnTypeInference : Error {}; // A9902_NotSupported_LambdaReturnTypeInference
export struct Error_NotImplemented : Error {};
export struct Error_NotRechable: Error {};

} // namespace Citron
