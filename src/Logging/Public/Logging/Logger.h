#pragma once
#include "LoggingConfig.h"
#include <memory>

namespace Citron {

using SSyntaxPtr = std::shared_ptr<class SSyntax>;

class Logger
{
public:
    // 호출자에서 Set하는 것으로 한다. 함수가 끝나도 Syntax가 지속될 수 있기 때문이다
    LOGGING_API void SetSyntax(SSyntaxPtr syntax);



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
    //

    //LOGGING_API void Fatal_Cast_Failed();                   // A2201_Cast_Failed

    //LOGGING_API void Fatal_NewExp_TypeIsNotClass();               // A2601_NewExp_TypeIsNotClass
    //LOGGING_API void Fatal_NullLiteralExp_CantInferNullableType();        // A2701_NullLiteralExp_CantInferNullableType

    //// IrExp -> RExp Translation
    //LOGGING_API void Fatal_Reference_CantMakeReference();                 // A3001_Reference_CantMakeReference
    //LOGGING_API void Fatal_Reference_CantReferenceTempValue();            // A3002_Reference_CantReferenceTempValue
    //LOGGING_API void Fatal_Reference_UselessDereferenceReferencedValue(); // A3003_Reference_UselessDereferenceReferencedValue
    //LOGGING_API void Fatal_Reference_CantReferenceThis();                 // A3004_Reference_CantReferenceThis

    // Analyzer
    LOGGING_API void Fatal_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType(); // A0102_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType
    LOGGING_API void Fatal_VarDecl_LocalVarNameShouldBeUniqueWithinScope(); // A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope
    LOGGING_API void Fatal_VarDecl_GlobalVariableNameShouldBeUnique(); // A0104_VarDecl_GlobalVariableNameShouldBeUnique
    // LOGGING_API void Fatal_VarDecl_CantUseEnumElementAsDeclType(); // A0105_VarDecl_CantUseEnumElementAsDeclType
    LOGGING_API void Fatal_VarDecl_RefDeclNeedInitializer(); // A0106_VarDecl_RefDeclNeedInitializer
    // LOGGING_API void Fatal_VarDecl_RefInitializerUsedOnNonRefVarDecl(); // A0110_VarDecl_RefInitializerUsedOnNonRefVarDecl
    LOGGING_API void Fatal_VarDecl_LocalVarDeclNeedInitializer(); // A0111_VarDecl_LocalVarDeclNeedInitializer
    LOGGING_API void Fatal_VarDecl_RefDeclNeedLocationInitializer(); // A0112_VarDecl_RefDeclNeedLocationInitializer

    LOGGING_API void Fatal_VarDecl_UsingLocalVarInsteadOfVarWhenInitExpIsLocalInterface(); // A0113_VarDecl_UsingLocalVarInsteadOfVarWhenInitExpIsLocalInterface
    LOGGING_API void Fatal_VarDecl_UsingBoxPtrVarInsteadOfVarWhenInitExpIsBoxPtr(); // A0114_VarDecl_UsingBoxPtrVarInsteadOfVarWhenInitExpIsBoxPtr
    LOGGING_API void Fatal_VarDecl_UsingLocalPtrVarInsteadOfVarWhenInitExpIsLocalPtr(); // A0115_VarDecl_UsingLocalPtrVarInsteadOfVarWhenInitExpIsLocalPtr
    LOGGING_API void Fatal_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr(); // A0116_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr

    LOGGING_API void Fatal_VarDecl_UsingLocalVarAsDeclTypeButInitExpIsNotLocalInterface(); // A0117_VarDecl_UsingLocalVarAsDeclTypeButInitExpIsNotLocalInterface
    LOGGING_API void Fatal_VarDecl_UsingBoxPtrVarAsDeclTypeButInitExpIsNotBoxPtr(); // A0118_VarDecl_UsingBoxPtrVarAsDeclTypeButInitExpIsNotBoxPtr
    LOGGING_API void Fatal_VarDecl_UsingLocalPtrVarAsDeclTypeButInitExpIsNotLocalPtr(); // A0119_VarDecl_UsingLocalPtrVarAsDeclTypeButInitExpIsNotLocalPtr
    LOGGING_API void Fatal_VarDecl_UsingNullableVarAsDeclTypeButInitExpIsNotNullable(); // A0120_VarDecl_UsingNullableVarAsDeclTypeButInitExpIsNotNullable

    LOGGING_API void Fatal_VarDecl_InitExpTypeMismatch(); // A0121_VarDecl_InitExpTypeMismatch


    LOGGING_API void Fatal_Capturer_ReferencingLocalVariableIsNotAllowed(); // A0201_Capturer_ReferencingLocalVariableIsNotAllowed

    // LOGGING_API void Fatal_MemberExp_InstanceTypeIsNotNormalType(); // A0301_MemberExp_InstanceTypeIsNotNormalType
    // LOGGING_API void Fatal_MemberExp_TypeArgsForMemberVariableIsNotAllowed(); // A0302_MemberExp_TypeArgsForMemberVariableIsNotAllowed
    // LOGGING_API void Fatal_MemberExp_MemberNotFound(); // A0303_MemberExp_MemberNotFound
    // LOGGING_API void Fatal_MemberExp_MemberVariableIsNotStatic(); // A0304_MemberExp_MemberVariableIsNotStatic
    // LOGGING_API void Fatal_MemberExp_MemberVariableIsStatic(); // A0305_MemberExp_MemberVariableIsStatic
    // LOGGING_API void Fatal_MemberExp_MultipleCandidates(); // A0306_MemberExp_MultipleCandidates
    // LOGGING_API void Fatal_MemberExp_MemberIsNotExpression(); // A0307_MemberExp_MemberIsNotExpression

    LOGGING_API void Fatal_Parameter_MismatchBetweenParamCountAndArgCount(); // A0401_Parameter_MismatchBetweenParamCountAndArgCount
    // LOGGING_API void Fatal_Parameter_MismatchBetweenParamTypeAndArgType(); // A0402_Parameter_MismatchBetweenParamTypeAndArgType => FuncMatcher..로 변경
    LOGGING_API void Fatal_Parameter_ParamsArgumentShouldBeTuple(); // A0402_Parameter_ParamsArgumentShouldBeTuple

    // LOGGING_API void Fatal_IdExp_VariableNotFound(); // A0501_IdExp_VariableNotFound
    // LOGGING_API void Fatal_IdExp_CantUseTypeAsExpression(); // A0502_IdExp_CantUseTypeAsExpression
    // LOGGING_API void Fatal_IdExp_MultipleCandidates(); // A0503_IdExp_MultipleCandidates

    LOGGING_API void Fatal_UnaryAssignOp_IntTypeIsAllowedOnly(); // A0601_UnaryAssignOp_IntTypeIsAllowedOnly
    LOGGING_API void Fatal_UnaryAssignOp_AssignableExpressionIsAllowedOnly(); // A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly

    LOGGING_API void Fatal_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly(); // A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly
    LOGGING_API void Fatal_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly(); // A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly

    // LOGGING_API void Fatal_BinaryOp_LeftOperandTypeIsNotCompatibleWithRightOperandType(); // A0801_BinaryOp_LeftOperandTypeIsNotCompatibleWithRightOperandType => CastFailed로 변경
    LOGGING_API void Fatal_BinaryOp_OperatorNotFound(); // A0802_BinaryOp_OperatorNotFound
    LOGGING_API void Fatal_BinaryOp_LeftOperandIsNotAssignable(); // A0803_BinaryOp_LeftOperandIsNotAssignable

    LOGGING_API void Fatal_CallExp_MultipleCandidates(); // A0901_CallExp_MultipleCandidates
    LOGGING_API void Fatal_CallExp_CallableExpressionIsNotCallable(); // A0902_CallExp_CallableExpressionIsNotCallable
    LOGGING_API void Fatal_CallExp_MismatchEnumConstructorArgCount(); // A0903_CallExp_MismatchEnumConstructorArgCount
    LOGGING_API void Fatal_CallExp_MismatchBetweenEnumParamTypeAndEnumArgType(); // A0904_CallExp_MismatchBetweenEnumParamTypeAndEnumArgType
    LOGGING_API void Fatal_CallExp_NoMatchedStructConstructorFound(); // A0905_CallExp_NoMatchedStructConstructorFound // TODO: A2602_NewExp_NoConstructorFound 랑 겹침
    LOGGING_API void Fatal_CallExp_NotFound(); // A0906_CallExp_NotFound
    LOGGING_API void Fatal_CallExp_MultipleMatchedStructConstructors(); // A0907_CallExp_MultipleMatchedStructConstructors
    LOGGING_API void Fatal_CallExp_InstanceIsNotLocation(); // A0908_CallExp_InstanceIsNotLocation

    // LOGGING_API void Fatal_IfStmt_TestTargetShouldBeLocalVariable(); // A1001_IfStmt_TestTargetShouldBeLocalVariable
    // LOGGING_API void Fatal_IfStmt_TestTargetIdentifierNotFound(); // A1002_IfStmt_TestTargetIdentifierNotFound
    // LOGGING_API void Fatal_IfStmt_TestTypeShouldBeEnumOrClass(); // A1003_IfStmt_TestTypeShouldBeEnumOrClass
    LOGGING_API void Fatal_IfStmt_ConditionShouldBeBool(); // A1001_IfStmt_ConditionShouldBeBool

    LOGGING_API void Fatal_IfTestStmt_CantDowncast(); // A2301_IfTestStmt_CantDowncast

    LOGGING_API void Fatal_ForStmt_ConditionShouldBeBool(); // A1101_ForStmt_ConditionShouldBeBool
    LOGGING_API void Fatal_ForStmt_ExpInitializerShouldBeAssignOrCall(); // A1102_ForStmt_ExpInitializerShouldBeAssignOrCall
    LOGGING_API void Fatal_ForStmt_ContinueExpShouldBeAssignOrCall(); // A1103_ForStmt_ContinueExpShouldBeAssignOrCall

    LOGGING_API void Fatal_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType(); // A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType
    LOGGING_API void Fatal_ReturnStmt_SeqFuncShouldReturnVoid(); // A1202_ReturnStmt_SeqFuncShouldReturnVoid
    LOGGING_API void Fatal_ReturnStmt_RefTargetIsNotLocation(); // A1203_ReturnStmt_RefTargetIsNotLocation

    LOGGING_API void Fatal_ExpStmt_ExpressionShouldBeAssignOrCall(); // A1301_ExpStmt_ExpressionShouldBeAssignOrCall

    LOGGING_API void Fatal_YieldStmt_YieldShouldBeInSeqFunc(); // A1401_YieldStmt_YieldShouldBeInSeqFunc
    // LOGGING_API void Fatal_YieldStmt_MismatchBetweenYieldValueAndSeqFuncYieldType(); // A1402_YieldStmt_MismatchBetweenYieldValueAndSeqFuncYieldType => CastFailed

    LOGGING_API void Fatal_ContinueStmt_ShouldUsedInLoop(); // A1501_ContinueStmt_ShouldUsedInLoop

    LOGGING_API void Fatal_BreakStmt_ShouldUsedInLoop(); // A1601_BreakStmt_ShouldUsedInLoop

    LOGGING_API void Fatal_ListExp_CantInferElementTypeWithEmptyElement(); // A1701_ListExp_CantInferElementTypeWithEmptyElement
    LOGGING_API void Fatal_ListExp_MismatchBetweenElementTypes(); // A1702_ListExp_MismatchBetweenElementTypes

    LOGGING_API void Fatal_ForeachStmt_IteratorShouldBeListOrEnumerable(); // A1801_ForeachStmt_IteratorShouldBeListOrEnumerable // TODO: 추후에 더 일반적으로 바뀌어야 한다
    LOGGING_API void Fatal_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType(); // A1802_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType

    LOGGING_API void Fatal_StringExp_ExpElementShouldBeBoolOrIntOrString(); // A1901_StringExp_ExpElementShouldBeBoolOrIntOrString // TODO: 보다 일반적으로 바뀌어야 한다. ToString을 구현한 애들이 가능

    LOGGING_API void Fatal_ResolveIdentifier_MultipleCandidatesForIdentifier(); // A2001_ResolveIdentifier_MultipleCandidatesForIdentifier
    LOGGING_API void Fatal_ResolveIdentifier_VarWithTypeArg(); // A2002_ResolveIdentifier_VarWithTypeArg
    LOGGING_API void Fatal_ResolveIdentifier_CantGetStaticMemberThroughInstance(); // A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance // instance에서 static 함수, 변수를 가지고 오려고 했을 때, e.F, e.v (F, v가 static인 경우)
    LOGGING_API void Fatal_ResolveIdentifier_CantGetTypeMemberThroughInstance(); // A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance   // instance에서 타입을 가지고 오려고 했을 때, e.T
    LOGGING_API void Fatal_ResolveIdentifier_CantGetInstanceMemberThroughType(); // A2005_ResolveIdentifier_CantGetInstanceMemberThroughType
    LOGGING_API void Fatal_ResolveIdentifier_FuncCantHaveMember(); // A2006_ResolveIdentifier_FuncCantHaveMember
    LOGGING_API void Fatal_ResolveIdentifier_NotFound(); // A2007_ResolveIdentifier_NotFound

    LOGGING_API void Fatal_ResolveIdentifier_CantUseTypeAsExpression(); // A2008_ResolveIdentifier_CantUseTypeAsExpression // Type으로 Resolve는 되지만, 값으로 변경하려고 시도하다가 에러 var x = X.Y;

    LOGGING_API void Fatal_ResolveIdentifier_EnumElemCantHaveMember(); // A2009_ResolveIdentifier_EnumElemCantHaveMember
    LOGGING_API void Fatal_ResolveIdentifier_ThisIsNotInTheContext(); // A2010_ResolveIdentifier_ThisIsNotInTheContext   // 
    LOGGING_API void Fatal_ResolveIdentifier_TryAccessingPrivateMember(); // A2011_ResolveIdentifier_TryAccessingPrivateMember

    // LOGGING_API void Fatal_ResolveIdentifier_TypeVarCantHaveMember(); // A2012_ResolveIdentifier_TypeVarCantHaveMember -> interface를 지키면 멤버가 있을 수 있다
    LOGGING_API void Fatal_ResolveIdentifier_EnumInstanceCantHaveMember(); // A2013_ResolveIdentifier_EnumInstanceCantHaveMember

    LOGGING_API void Fatal_ResolveIdentifier_CantUseNamespaceAsExpression(); // A2013_ResolveIdentifier_CantUseNamespaceAsExpression // 네임스페이스를 exp로 쓸 수 없습니다        
    LOGGING_API void Fatal_ResolveIdentifier_MultipleCandidatesForMember(); // A2014_ResolveIdentifier_MultipleCandidatesForMember
    LOGGING_API void Fatal_ResolveIdentifier_ExpressionIsNotLocation(); // A2015_ResolveIdentifier_ExpressionIsNotLocation

    LOGGING_API void Fatal_ResolveIdentifier_LambdaInstanceCantHaveMember(); // A2016_ResolveIdentifier_LambdaInstanceCantHaveMember
    LOGGING_API void Fatal_ResolveIdentifier_LocalPtrCantHaveMember(); // A2017_ResolveIdentifier_LocalPtrCantHaveMember
    LOGGING_API void Fatal_ResolveIdentifier_BoxPtrCantHaveMember(); // A2018_ResolveIdentifier_BoxPtrCantHaveMember
    LOGGING_API void Fatal_ResolveIdentifier_FuncInstanceCantHaveMember(); // A2019_ResolveIdentifier_FuncInstanceCantHaveMember

    // funcMatcher 자체에서 에러를 내지 않고, 각각 노드 처리기에서 에러를 생성하도록 한다
    //LOGGING_API void Fatal_FuncMatcher_MultipleCandidates(); // A2101_FuncMatcher_MultipleCandidates 
    //LOGGING_API void Fatal_FuncMatcher_NotFound(); // A2102_FuncMatcher_NotFound

    LOGGING_API void Fatal_Cast_Failed(); // A2201_Cast_Failed

    LOGGING_API void Fatal_RootDecl_CannotSetPrivateAccessExplicitlyBecauseItsDefault(); // A2301_RootDecl_CannotSetPrivateAccessExplicitlyBecauseItsDefault

    LOGGING_API void Fatal_StructDecl_CannotSetMemberPublicAccessExplicitlyBecauseItsDefault(); // A2401_StructDecl_CannotSetMemberPublicAccessExplicitlyBecauseItsDefault
    LOGGING_API void Fatal_StructDecl_CannotSetMemberProtectedAccessBecauseItsNotAllowed(); // A2402_StructDecl_CannotSetMemberProtectedAccessBecauseItsNotAllowed
    LOGGING_API void Fatal_StructDecl_CannotDeclConstructorDifferentWithTypeName(); // A2403_StructDecl_CannotDeclConstructorDifferentWithTypeName

    LOGGING_API void Fatal_ClassDecl_CannotSetMemberPrivateAccessExplicitlyBecauseItsDefault(); // A2501_ClassDecl_CannotSetMemberPrivateAccessExplicitlyBecauseItsDefault
    LOGGING_API void Fatal_ClassDecl_CannotDeclConstructorDifferentWithTypeName(); // A2502_ClassDecl_CannotDeclConstructorDifferentWithTypeName
    LOGGING_API void Fatal_ClassDecl_CannotFindBaseClassConstructor(); // A2503_ClassDecl_CannotFindBaseClassConstructor
    LOGGING_API void Fatal_ClassDecl_CannotAccessBaseClassConstructor(); // A2504_ClassDecl_CannotAccessBaseClassConstructor
    LOGGING_API void Fatal_ClassDecl_TryCallBaseConstructorWithoutBaseClass(); // A2505_ClassDecl_TryCallBaseConstructorWithoutBaseClass
    LOGGING_API void Fatal_ClassDecl_CannotDecideWhichBaseConstructorUse(); // A2506_ClassDecl_CannotDecideWhichBaseConstructorUse

    LOGGING_API void Fatal_NewExp_TypeIsNotClass(); // A2601_NewExp_TypeIsNotClass
    LOGGING_API void Fatal_NewExp_NoMatchedClassConstructor(); // A2602_NewExp_NoMatchedClassConstructor // TODO: LOGGING_API void Fatal_CallExp_NoConstructorFound(); // A0905_CallExp_NoConstructorFound 랑 겹침
    LOGGING_API void Fatal_NewExp_MultipleMatchedClassConstructors(); // A2603_NewExp_MultipleMatchedClassConstructors // TODO: LOGGING_API void Fatal_CallExp_NoConstructorFound(); // A0905_CallExp_NoConstructorFound 랑 겹침

    LOGGING_API void Fatal_NullLiteralExp_CantInferNullableType(); // A2701_NullLiteralExp_CantInferNullableType // null은 힌트와 쓰인다

    // LOGGING_API void Fatal_Identifier_MultipleCandidatesForIdentifier(); // A2001_Identifier_MultipleCandidatesForIdentifier

    LOGGING_API void Fatal_StaticNotNullDirective_ShouldHaveOneArgument(); // A2801_StaticNotNullDirective_ShouldHaveOneArgument
    LOGGING_API void Fatal_StaticNotNullDirective_ArgumentMustBeLocation(); // A2802_StaticNotNullDirective_ArgumentMustBeLocation

    // A2901_BodyShouldReturn, // 리턴 타입이 있는 본문에 리턴이 없다

    LOGGING_API void Fatal_Reference_CantMakeReference(); // A3001_Reference_CantMakeReference
    LOGGING_API void Fatal_Reference_CantReferenceTempValue(); // A3002_Reference_CantReferenceTempValue
    LOGGING_API void Fatal_Reference_UselessDereferenceReferencedValue(); // A3003_Reference_UselessDereferenceReferencedValue
    LOGGING_API void Fatal_Reference_CantReferenceThis(); // A3004_Reference_CantReferenceThis

    LOGGING_API void Fatal_NotSupported_LambdaParameterInference(); // A9901_NotSupported_LambdaParameterInference
    LOGGING_API void Fatal_NotSupported_LambdaReturnTypeInference(); // A9902_NotSupported_LambdaReturnTypeInference


};

using LoggerPtr = std::shared_ptr<Logger>;

}