using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Analysis
{
    // 에러 코드는 여기서 모두 관리한다
    public enum SyntaxAnalysisErrorCode
    {
        // Analyzer
        A0102_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType,
        A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope,
        A0104_VarDecl_GlobalVariableNameShouldBeUnique,
        // A0105_VarDecl_CantUseEnumElementAsDeclType,
        A0106_VarDecl_RefDeclNeedInitializer,
        // A0110_VarDecl_RefInitializerUsedOnNonRefVarDecl,
        A0111_VarDecl_LocalVarDeclNeedInitializer,
        A0112_VarDecl_RefDeclNeedLocationInitializer,

        A0201_Capturer_ReferencingLocalVariableIsNotAllowed,

        // A0301_MemberExp_InstanceTypeIsNotNormalType,
        // A0302_MemberExp_TypeArgsForMemberVariableIsNotAllowed,
        // A0303_MemberExp_MemberNotFound,
        // A0304_MemberExp_MemberVariableIsNotStatic,
        // A0305_MemberExp_MemberVariableIsStatic,
        // A0306_MemberExp_MultipleCandidates,
        // A0307_MemberExp_MemberIsNotExpression,

        A0401_Parameter_MismatchBetweenParamCountAndArgCount,
        // A0402_Parameter_MismatchBetweenParamTypeAndArgType, => FuncMatcher..로 변경

        // A0501_IdExp_VariableNotFound,
        // A0502_IdExp_CantUseTypeAsExpression,
        // A0503_IdExp_MultipleCandidates,

        A0601_UnaryAssignOp_IntTypeIsAllowedOnly,
        A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly,

        A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly,
        A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly,

        // A0801_BinaryOp_LeftOperandTypeIsNotCompatibleWithRightOperandType, => CastFailed로 변경
        A0802_BinaryOp_OperatorNotFound,
        A0803_BinaryOp_LeftOperandIsNotAssignable,

        A0901_CallExp_MultipleCandidates,
        A0902_CallExp_CallableExpressionIsNotCallable,
        A0903_CallExp_MismatchEnumConstructorArgCount,
        A0904_CallExp_MismatchBetweenEnumParamTypeAndEnumArgType,
        A0905_CallExp_NoMatchedStructConstructorFound, // TODO: A2602_NewExp_NoConstructorFound 랑 겹침
        A0906_CallExp_NotFound,
        A0907_CallExp_MultipleMatchedStructConstructors,

        // A1001_IfStmt_TestTargetShouldBeLocalVariable,
        // A1002_IfStmt_TestTargetIdentifierNotFound,
        // A1003_IfStmt_TestTypeShouldBeEnumOrClass,
        A1001_IfStmt_ConditionShouldBeBool,

        A2301_IfTestStmt_CantDowncast,

        A1101_ForStmt_ConditionShouldBeBool,
        A1102_ForStmt_ExpInitializerShouldBeAssignOrCall,
        A1103_ForStmt_ContinueExpShouldBeAssignOrCall,

        A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType,
        A1202_ReturnStmt_SeqFuncShouldReturnVoid,
        A1203_ReturnStmt_RefTargetIsNotLocation,

        A1301_ExpStmt_ExpressionShouldBeAssignOrCall,

        A1401_YieldStmt_YieldShouldBeInSeqFunc,
        // A1402_YieldStmt_MismatchBetweenYieldValueAndSeqFuncYieldType, => CastFailed

        A1501_ContinueStmt_ShouldUsedInLoop,

        A1601_BreakStmt_ShouldUsedInLoop,

        A1701_ListExp_CantInferElementTypeWithEmptyElement,
        A1702_ListExp_MismatchBetweenElementTypes,

        A1801_ForeachStmt_IteratorShouldBeListOrEnumerable, // TODO: 추후에 더 일반적으로 바뀌어야 한다
        A1802_ForeachStmt_MismatchBetweenElemTypeAndIteratorElemType,

        A1901_StringExp_ExpElementShouldBeBoolOrIntOrString, // TODO: 보다 일반적으로 바뀌어야 한다. ToString을 구현한 애들이 가능

        A2001_ResolveIdentifier_MultipleCandidatesForIdentifier,
        A2002_ResolveIdentifier_VarWithTypeArg,
        A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, // instance에서 static 함수, 변수를 가지고 오려고 했을 때, e.F, e.v (F, v가 static인 경우)
        A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance,   // instance에서 타입을 가지고 오려고 했을 때, e.T
        A2005_ResolveIdentifier_CantGetInstanceMemberThroughType,
        A2006_ResolveIdentifier_FuncCantHaveMember,
        A2007_ResolveIdentifier_NotFound,
        A2008_ResolveIdentifier_CantUseTypeAsExpression, // Type으로 Resolve는 되지만, 값으로 변경하려고 시도하다가 에러 var x = X.Y;
        A2009_ResolveIdentifier_EnumElemCantHaveMember,
        A2010_ResolveIdentifier_ThisIsNotInTheContext,   // 
        A2011_ResolveIdentifier_TryAccessingPrivateMember,
        A2012_ResolveIdentifier_TypeVarCantHaveMember,

        // funcMatcher 자체에서 에러를 내지 않고, 각각 노드 처리기에서 에러를 생성하도록 한다
        //A2101_FuncMatcher_MultipleCandidates, 
        //A2102_FuncMatcher_NotFound,

        A2201_Cast_Failed,

        A2301_RootDecl_CannotSetPrivateAccessExplicitlyBecauseItsDefault,

        A2401_StructDecl_CannotSetMemberPublicAccessExplicitlyBecauseItsDefault,
        A2402_StructDecl_CannotSetMemberProtectedAccessBecauseItsNotAllowed,
        A2403_StructDecl_CannotDeclConstructorDifferentWithTypeName,

        A2501_ClassDecl_CannotSetMemberPrivateAccessExplicitlyBecauseItsDefault,
        A2502_ClassDecl_CannotDeclConstructorDifferentWithTypeName,
        A2503_ClassDecl_CannotFindBaseClassConstructor,
        A2504_ClassDecl_CannotAccessBaseClassConstructor,
        A2505_ClassDecl_TryCallBaseConstructorWithoutBaseClass,
        A2506_ClassDecl_CannotDecideWhichBaseConstructorUse,

        A2601_NewExp_TypeIsNotClass,
        A2602_NewExp_NoMatchedClassConstructor, // TODO: A0905_CallExp_NoConstructorFound, 랑 겹침
        A2603_NewExp_MultipleMatchedClassConstructors, // TODO: A0905_CallExp_NoConstructorFound, 랑 겹침

        A2701_NullLiteralExp_CantInferNullableType, // null은 힌트와 쓰인다

        // A2001_Identifier_MultipleCandidatesForIdentifier,

        A2801_StaticNotNullDirective_ShouldHaveOneArgument,
        A2802_StaticNotNullDirective_ArgumentMustBeLocation,

        A2901_BodyShouldReturn, // 리턴 타입이 있는 본문에 리턴이 없다

        A9901_NotSupported_LambdaParameterInference,
        A9902_NotSupported_LambdaReturnTypeInference,
    }
}
