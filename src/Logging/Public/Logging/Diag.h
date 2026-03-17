#pragma once
#include "LoggingConfig.h"

#include <variant>
#include <memory>
#include <vector>

namespace Citron {

struct Diag 
{
    virtual ~Diag() = default;
};
struct WarnDiag : Diag {};
struct ErrorDiag : Diag 
{
    ErrorDiag() {}
};

using DiagPtr = std::shared_ptr<Diag>;

struct NestedDiag : Diag
{
    DiagPtr diag;
    DiagPtr child;
};

struct AggregateDiag : Diag 
{
    std::vector<DiagPtr> diags;
    AggregateDiag(std::vector<DiagPtr>&& diags) : diags(std::move(diags)) { }
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
//LOGGING_API void Fatal_ResolveIdentifier_PtrCantHaveMember();                  // A2017_ResolveIdentifier_PtrCantHaveMember
//LOGGING_API void Fatal_ResolveIdentifier_SharedCantHaveMember();                  // A2018_ResolveIdentifier_BoxCantHaveMember
//LOGGING_API void Fatal_ResolveIdentifier_FuncInstanceCantHaveMember();         // A2019_ResolveIdentifier_FuncInstanceCantHaveMember

//LOGGING_API void Fatal_Cast_Failed();                   // A2201_Cast_Failed

//LOGGING_API void Fatal_NewExp_TypeIsNotClass();               // A2601_NewExp_TypeIsNotClass
//LOGGING_API void Fatal_NullLiteralExp_CantInferNullableType();        // A2701_NullLiteralExp_CantInferNullableType

//// IrExp -> MExp Translation
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
// LOGGING_API void Fatal_Parameter_MismatchBetweenParamTypeAndArgType(); // A0403_Parameter_MismatchBetweenParamTypeAndArgType => FuncMatcher..로 변경
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

struct Error_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType : ErrorDiag { }; // A0102_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType
struct Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope : ErrorDiag { }; // A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope
struct Error_VarDecl_GlobalVariableNameShouldBeUnique : ErrorDiag { }; // A0104_VarDecl_GlobalVariableNameShouldBeUnique

struct Error_VarDecl_RefDeclCantUsingMove : ErrorDiag { };
struct Error_VarDecl_RefDeclNeedLocationInitializer : ErrorDiag {}; // A0112_VarDecl_RefDeclNeedLocationInitializer


struct Error_VarDecl_LocalVarDeclNeedInitializer : ErrorDiag { }; // A0111_VarDecl_LocalVarDeclNeedInitializer

struct Error_VarDecl_UsingLocalVarInsteadOfVarWhenInitExpIsLocalInterface : ErrorDiag { }; // A0113_VarDecl_UsingLocalVarInsteadOfVarWhenInitExpIsLocalInterface
struct Error_VarDecl_UsingSharedVarInsteadOfVarWhenInitExpIsShared : ErrorDiag {}; 
struct Error_VarDecl_UsingBoxVarInsteadOfVarWhenInitExpIsBox : ErrorDiag { }; // A0114_VarDecl_UsingBoxVarInsteadOfVarWhenInitExpIsBox
struct Error_VarDecl_UsingPtrVarInsteadOfVarWhenInitExpIsPtr : ErrorDiag { }; // A0115_VarDecl_UsingPtrVarrInsteadOfVarWhenInitExpIsPtr
struct Error_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr : ErrorDiag { }; // A0116_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr
struct Error_VarDecl_UsingLocalVarAsDeclTypeButInitExpIsNotLocalInterface : ErrorDiag { }; // A0117_VarDecl_UsingLocalVarAsDeclTypeButInitExpIsNotLocalInterface
struct Error_VarDecl_UsingSharedVarAsDeclTypeButInitExpIsNotShared : ErrorDiag {}; // 
struct Error_VarDecl_UsingBoxVarAsDeclTypeButInitExpIsNotBox : ErrorDiag { }; // A0118_VarDecl_UsingBoxVarAsDeclTypeButInitExpIsNotBox
struct Error_VarDecl_UsingPtrVarAsDeclTypeButInitExpIsNotPtr : ErrorDiag { }; // A0119_VarDecl_UsingPtrVarAsDeclTypeButInitExpIsNotPtr
struct Error_VarDecl_UsingNullableVarAsDeclTypeButInitExpIsNotNullable : ErrorDiag { }; // A0120_VarDecl_UsingNullableVarAsDeclTypeButInitExpIsNotNullable
struct Error_VarDecl_InitExpTypeMismatch : ErrorDiag { }; // A0121_VarDecl_InitExpTypeMismatch
struct Error_VarDecl_CantInferenceWithoutInitExpression : ErrorDiag { }; 

struct Error_Capturer_ReferencingLocalVariableIsNotAllowed : ErrorDiag { }; // A0201_Capturer_ReferencingLocalVariableIsNotAllowed



struct Error_FuncMatch_MultipleCandidates : ErrorDiag {}; // A0401
struct Error_FuncMatch_NotFound : ErrorDiag {}; // A0402
struct Error_FuncMatch_MismatchBetweenParamCountAndArgCount : ErrorDiag {}; // A0403
struct Error_FuncMatch_ParamsArgumentShouldBeTuple : ErrorDiag {}; // A0404
struct Error_FuncMatch_MismatchBetweenParamTypeAndArgType : ErrorDiag {}; // A0405

struct Error_FuncDecl_ParameterKindNeedRef : ErrorDiag {}; // [in] [move] [forward] [out] 일 경우 타입에 &를 붙여야 한다

struct Error_UnaryAssignOp_IntTypeIsAllowedOnly : ErrorDiag { }; // A0601_UnaryAssignOp_IntTypeIsAllowedOnly
struct Error_UnaryAssignOp_AssignableExpressionIsAllowedOnly : ErrorDiag { }; // A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly
struct Error_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly : ErrorDiag { }; // A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly
struct Error_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly : ErrorDiag {}; // A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly
struct Error_UnaryOp_RefNeedHintType : ErrorDiag {}; // A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly
struct Error_UnaryOp_RefHintTypeShouldBePtrOrShared : ErrorDiag { }; // A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly

struct Error_BinaryOp_OperatorNotFound : ErrorDiag { }; // A0802_BinaryOp_OperatorNotFound
struct Error_BinaryOp_LeftOperandIsNotAssignable : ErrorDiag { }; // A0803_BinaryOp_LeftOperandIsNotAssignable
struct Error_CallExp_MultipleCandidates : ErrorDiag { }; // A0901_CallExp_MultipleCandidates
struct Error_CallExp_CallableExpressionIsNotCallable : ErrorDiag { }; // A0902_CallExp_CallableExpressionIsNotCallable
struct Error_CallExp_MismatchEnumCtorArgCount : ErrorDiag { }; // A0903_CallExp_MismatchEnumCtorArgCount
struct Error_CallExp_MismatchBetweenEnumParamTypeAndEnumArgType : ErrorDiag { }; // A0904_CallExp_MismatchBetweenEnumParamTypeAndEnumArgType
struct Error_CallExp_NoMatchedStructCtorFound : ErrorDiag { }; // A0905_CallExp_NoMatchedStructCtorFound // TODO: A2602_NewExp_NoCtorFound 랑 겹침
struct Error_CallExp_NotFound : ErrorDiag { }; // A0906_CallExp_NotFound
struct Error_CallExp_MultipleMatchedStructCtors : ErrorDiag { }; // A0907_CallExp_MultipleMatchedStructCtors
struct Error_CallExp_InstanceIsNotLocation : ErrorDiag { }; // A0908_CallExp_InstanceIsNotLocation
struct Error_IfStmt_ConditionShouldBeBool : ErrorDiag { }; // A1001_IfStmt_ConditionShouldBeBool
struct Error_IfTestStmt_CantDowncast : ErrorDiag { }; // A2301_IfTestStmt_CantDowncast
struct Error_ForStmt_ConditionShouldBeBool : ErrorDiag { }; // A1101_ForStmt_ConditionShouldBeBool
struct Error_ForStmt_ExpInitializerShouldBeAssignOrCall : ErrorDiag { }; // A1102_ForStmt_ExpInitializerShouldBeAssignOrCall
struct Error_ForStmt_ContinueExpShouldBeAssignOrCall : ErrorDiag { }; // A1103_ForStmt_ContinueExpShouldBeAssignOrCall
struct Error_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType : ErrorDiag { }; // A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType
struct Error_ReturnStmt_SeqFuncShouldReturnVoid : ErrorDiag { }; // A1202_ReturnStmt_SeqFuncShouldReturnVoid
struct Error_ReturnStmt_RefTargetIsNotLocation : ErrorDiag { }; // A1203_ReturnStmt_RefTargetIsNotLocation

struct Error_ExpStmt_ExpressionShouldBeAssignOrCall : ErrorDiag { }; // A1301_ExpStmt_ExpressionShouldBeAssignOrCall

struct Error_YieldStmt_YieldShouldBeInSeqFunc : ErrorDiag { }; // A1401_YieldStmt_YieldShouldBeInSeqFunc
struct Error_ContinueStmt_ShouldUsedInLoop : ErrorDiag { }; // A1501_ContinueStmt_ShouldUsedInLoop
struct Error_BreakStmt_ShouldUsedInLoop : ErrorDiag { }; // A1601_BreakStmt_ShouldUsedInLoop
struct Error_ListExp_CantInferElementTypeWithEmptyElement : ErrorDiag { }; // A1701_ListExp_CantInferElementTypeWithEmptyElement
struct Error_ListExp_MismatchBetweenElementTypes : ErrorDiag { }; // A1702_ListExp_MismatchBetweenElementTypes
struct Error_ForeachStmt_IteratorShouldBeListOrEnumerable : ErrorDiag { }; // A1801_ForeachStmt_IteratorShouldBeListOrEnumerable // TODO: 추후에 더 일반적으로 바뀌어야 한다
struct Error_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType : ErrorDiag { }; // A1802_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType
struct Error_StringExp_ExpElementShouldBeBoolOrIntOrString : ErrorDiag { }; // A1901_StringExp_ExpElementShouldBeBoolOrIntOrString // TODO: 보다 일반적으로 바뀌어야 한다. ToString을 구현한 애들이 가능
struct Error_ResolveIdentifier_MultipleCandidatesForIdentifier : ErrorDiag { }; // A2001_ResolveIdentifier_MultipleCandidatesForIdentifier
struct Error_ResolveIdentifier_VarWithTypeArg : ErrorDiag { }; // A2002_ResolveIdentifier_VarWithTypeArg
struct Error_ResolveIdentifier_CantGetStaticMemberThroughInstance : ErrorDiag { }; // A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance // instance에서 static 함수, 변수를 가지고 오려고 했을 때, e.F, e.v (F, v가 static인 경우)
struct Error_ResolveIdentifier_CantGetTypeMemberThroughInstance : ErrorDiag { }; // A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance   // instance에서 타입을 가지고 오려고 했을 때, e.T
struct Error_ResolveIdentifier_CantGetInstanceMemberThroughType : ErrorDiag { }; // A2005_ResolveIdentifier_CantGetInstanceMemberThroughType
struct Error_ResolveIdentifier_FuncCantHaveMember : ErrorDiag { }; // A2006_ResolveIdentifier_FuncCantHaveMember
struct Error_ResolveIdentifier_NotFound : ErrorDiag { }; // A2007_ResolveIdentifier_NotFound
struct Error_ResolveIdentifier_CantUseTypeAsExpression : ErrorDiag { }; // A2008_ResolveIdentifier_CantUseTypeAsExpression // Type으로 Resolve는 되지만, 값으로 변경하려고 시도하다가 에러 var x = X.Y;
struct Error_ResolveIdentifier_EnumElemCantHaveMember : ErrorDiag { }; // A2009_ResolveIdentifier_EnumElemCantHaveMember
struct Error_ResolveIdentifier_ThisIsNotInTheContext : ErrorDiag { }; // A2010_ResolveIdentifier_ThisIsNotInTheContext   // 
struct Error_ResolveIdentifier_TryAccessingPrivateMember : ErrorDiag { }; // A2011_ResolveIdentifier_TryAccessingPrivateMember
struct Error_ResolveIdentifier_EnumInstanceCantHaveMember : ErrorDiag { }; // A2013_ResolveIdentifier_EnumInstanceCantHaveMember
struct Error_ResolveIdentifier_CantUseNamespaceAsExpression : ErrorDiag { }; // A2013_ResolveIdentifier_CantUseNamespaceAsExpression // 네임스페이스를 exp로 쓸 수 없습니다        
struct Error_ResolveIdentifier_MultipleCandidatesForMember : ErrorDiag { }; // A2014_ResolveIdentifier_MultipleCandidatesForMember
struct Error_ResolveIdentifier_ExpressionIsNotLocation : ErrorDiag { }; // A2015_ResolveIdentifier_ExpressionIsNotLocation
struct Error_ResolveIdentifier_LambdaInstanceCantHaveMember : ErrorDiag { }; // A2016_ResolveIdentifier_LambdaInstanceCantHaveMember
struct Error_ResolveIdentifier_PtrCantHaveMember : ErrorDiag { }; // A2017_ResolveIdentifier_PtrCantHaveMember
struct Error_ResolveIdentifier_BoxCantHaveMember : ErrorDiag {}; // A2018_ResolveIdentifier_BoxCantHaveMember
struct Error_ResolveIdentifier_SharedCantHaveMember : ErrorDiag {}; // A2019_ResolveIdentifier_SharedCantHaveMember
struct Error_ResolveIdentifier_FuncInstanceCantHaveMember : ErrorDiag { }; // A2020_ResolveIdentifier_FuncInstanceCantHaveMember
struct Error_ResolveIdentifier_MemberBaseCantBeLocation : ErrorDiag {};

struct Error_SharedTranslation_SingleRefNotAllowed : ErrorDiag {}; // shared<int> i = &c; 금지
struct Error_SharedTranslation_MemberBaseShouldBeShared : ErrorDiag {}; // &s.x 금지 (s가 struct S일때)

struct Error_SharedTranslation_CantMakeSharedFromBase : ErrorDiag {}; // &e.x에서 e로 shared를 만들수 없을때
struct Error_SharedTranslation_StaticSharedShouldBeVar : ErrorDiag {}; // &C.x에서 x가 var가 아닐때

struct Error_SharedTranslation_CantTranslateToLoc : ErrorDiag {};
struct Error_SharedTranslation_CantTranslateToMSharedExp : ErrorDiag {};

struct Error_SharedTranslation_CantTranslate : ErrorDiag {};

struct Error_Indexer_ObjectShouldBeListOrDictionary : ErrorDiag {};
struct Error_Indexer_IndexTypeNotMatched : ErrorDiag {};

struct Error_As_NotSupported : ErrorDiag {};

struct Error_Cast_Failed : ErrorDiag { }; // A2201_Cast_Failed
struct Error_RootDecl_CannotSetPrivateAccessExplicitlyBecauseItsDefault : ErrorDiag { }; // A2301_RootDecl_CannotSetPrivateAccessExplicitlyBecauseItsDefault
struct Error_StructDecl_CannotSetMemberPublicAccessExplicitlyBecauseItsDefault : ErrorDiag { }; // A2401_StructDecl_CannotSetMemberPublicAccessExplicitlyBecauseItsDefault
struct Error_StructDecl_CannotSetMemberProtectedAccessBecauseItsNotAllowed : ErrorDiag { }; // A2402_StructDecl_CannotSetMemberProtectedAccessBecauseItsNotAllowed
struct Error_StructDecl_CannotDeclCtorDifferentWithTypeName : ErrorDiag { }; // A2403_StructDecl_CannotDeclCtorDifferentWithTypeName
struct Error_ClassDecl_CannotSetMemberPrivateAccessExplicitlyBecauseItsDefault : ErrorDiag { }; // A2501_ClassDecl_CannotSetMemberPrivateAccessExplicitlyBecauseItsDefault
struct Error_ClassDecl_CannotDeclCtorDifferentWithTypeName : ErrorDiag { }; // A2502_ClassDecl_CannotDeclCtorDifferentWithTypeName
struct Error_ClassDecl_CannotFindBaseClassCtor : ErrorDiag { }; // A2503_ClassDecl_CannotFindBaseClassCtor
struct Error_ClassDecl_CannotAccessBaseClassCtor : ErrorDiag { }; // A2504_ClassDecl_CannotAccessBaseClassCtor
struct Error_ClassDecl_TryCallBaseCtorWithoutBaseClass : ErrorDiag { }; // A2505_ClassDecl_TryCallBaseCtorWithoutBaseClass
struct Error_ClassDecl_CannotDecideWhichBaseCtorUse : ErrorDiag { }; // A2506_ClassDecl_CannotDecideWhichBaseCtorUse
struct Error_NewExp_TypeIsNotClass : ErrorDiag { }; // A2601_NewExp_TypeIsNotClass
struct Error_NewExp_NoMatchedClassCtor : ErrorDiag { }; // A2602_NewExp_NoMatchedClassCtor // TODO: LOGGING_API void Fatal_CallExp_NoConstructorFound(); // A0905_CallExp_NoConstructorFound 랑 겹침
struct Error_NewExp_MultipleMatchedClassCtors : ErrorDiag { }; // A2603_NewExp_MultipleMatchedClassCtors // TODO: LOGGING_API void Fatal_CallExp_NoCtorFound(); // A0905_CallExp_NoConstructorFound 랑 겹침
struct Error_NullLiteralExp_CantInferNullableType : ErrorDiag { }; // A2701_NullLiteralExp_CantInferNullableType // null은 힌트와 쓰인다
struct Error_StaticNotNullDirective_ShouldHaveOneArgument : ErrorDiag { }; // A2801_StaticNotNullDirective_ShouldHaveOneArgument
struct Error_StaticNotNullDirective_ArgumentMustBeLocation : ErrorDiag { }; // A2802_StaticNotNullDirective_ArgumentMustBeLocation

//struct Error_Reference_CantMakeReference : ErrorDiag { }; // A3001_Reference_CantMakeReference
//struct Error_Reference_CantReferenceTempValue : ErrorDiag { }; // A3002_Reference_CantReferenceTempValue
//struct Error_Reference_UselessDereferenceReferencedValue : ErrorDiag { }; // A3003_Reference_UselessDereferenceReferencedValue
//struct Error_Reference_CantReferenceThis : ErrorDiag { }; // A3004_Reference_CantReferenceThis

struct Error_Argument_Ref_ArgIsNotLoc : ErrorDiag {};
struct Error_Argument_Ref_ParamIsNotRef : ErrorDiag {};
struct Error_Argument_Move_ArgIsNotLoc : ErrorDiag {};
struct Error_Argument_Move_ParamMismatch : ErrorDiag {}; // 함수의 파라미터가 [move], [params]일때만 가능
struct Error_Argument_Forward_ArgShouldBeForwardArg : ErrorDiag {}; // [forward]가 붙은 argument만 [forward] T&에 매칭 가능 void F([forward]T& t) { G(forward t); }
struct Error_Argument_Forward_ParamMismatch : ErrorDiag {}; // 함수의 파라미터가 [forward]일때만 가능 void G([forward]T& t); void F([forward]T& t) { G(forward t); }

struct Error_Argument_Mismatch_NormalRef_Exp : ErrorDiag {}; // T& 인자로 Exp(BC)가 들어온 경우
struct Error_Argument_Mismatch_NormalRef_InitExp : ErrorDiag {}; // T& 인자로 InitExp(NBC)가 들어온 경우
struct Error_Argument_Mismatch_MoveRef_LocBC : ErrorDiag {}; // [move]T& 인자로 Loc(BC)가 들어온 경우
struct Error_Argument_Mismatch_MoveRef_LocNBC : ErrorDiag {}; // [move]T& 인자로 Loc(NBC)가 들어온 경우
struct Error_Argument_StmtCall : ErrorDiag {}; // 어떤 파라미터라도 void call을 argument로 받을 수 없다
struct Error_Argument_StmtAssign : ErrorDiag {}; // 어떤 파라미터라도 NBC assign을 argument로 받을 수 없다

struct Error_NotSupported_LambdaParameterInference : ErrorDiag { }; // A9901_NotSupported_LambdaParameterInference
struct Error_NotSupported_LambdaReturnTypeInference : ErrorDiag {}; // A9902_NotSupported_LambdaReturnTypeInference

} // namespace Citron
