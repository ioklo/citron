using System;
using System.Collections.Generic;
using System.Text;
using Gum.Infra;

using S = Gum.Syntax;

namespace Gum.IR0
{
    public enum AnalyzeErrorCode
    {
        // TypeSkeletonCollector
        S0101_Failed,

        // TypeExpEvaluator
        T0101_IdTypeExp_TypeDoesntHaveTypeParams,
        T0102_IdTypeExp_VarTypeCantApplyTypeArgs,
        T0103_IdTypeExp_MultipleTypesOfSameName,
        T0104_IdTypeExp_TypeNotFound,

        T0201_MemberTypeExp_TypeIsNotNormalType,
        T0202_MemberTypeExp_MemberTypeNotFound,


        // Analyzer
        A0101_VarDecl_CantInferVarType,
        A0102_VarDecl_MismatchBetweenDeclTypeAndInitExpType,

        A0201_Capturer_ReferencingLocalVariableIsNotAllowed,

        A0301_MemberExp_InstanceTypeIsNotNormalType,
        A0302_MemberExp_TypeArgsForMemberVariableIsNotAllowed,
        A0303_MemberExp_MemberVarNotFound,
        A0304_MemberExp_MemberVariableIsNotStatic,

        A0401_Parameter_MismatchBetweenParamCountAndArgCount,
        A0402_Parameter_MismatchBetweenParamTypeAndArgType,

        A0501_IdExp_VariableNotFound,

        A0601_UnaryAssignOp_IntTypeIsAllowedOnly,
        A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly,

        A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly,
        A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly,

        A0801_BinaryOp_LeftOperandTypeIsNotCompatibleWithRightOperandType,
        A0802_BinaryOp_OperatorNotFound,

        A0901_CallExp_ThereAreMultipleGlobalFunctionsHavingSameSignature,
        A0902_CallExp_CallableExpressionIsNotCallable,
        A0903_CallExp_MismatchEnumConstructorArgCount,
        A0903_CallExp_MismatchBetweenEnumParamTypeAndEnumArgType,

        A1001_IfStmt_TestTargetShouldBeVariable,
        A1002_IfStmt_TestTargetIdentifierNotFound,
        A1003_IfStmt_TestTypeShouldBeEnumOrClass,
        A1004_IfStmt_ConditionShouldBeBool,

        A1104_ForStmt_ConditionShouldBeBool,

        A1201_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType,
        A1202_ReturnStmt_SeqFuncShouldReturnVoid,

        A1301_ExpStmt_ExpressionShouldBeAssignOrCall,        

        A1401_YieldStmt_YieldShouldBeInSeqFunc,
        A1402_YieldStmt_MismatchBetweenYieldValueAndSeqFuncYieldType,


        A9901_NotSupported_LambdaParameterInference

    }

    public class AnalyzeError : IError
    {
        public AnalyzeErrorCode Code { get; }
        public S.ISyntaxNode Node { get; }
        public string Message { get; }

        public AnalyzeError(AnalyzeErrorCode code, S.ISyntaxNode node, string msg)
        {
            (Code, Node, Message) = (code, node, msg);
        }
    }
}
