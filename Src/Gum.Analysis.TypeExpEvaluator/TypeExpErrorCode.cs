namespace Citron.Analysis
{
    public enum TypeExpErrorCode
    {
        // TypeExpEvaluator
        T0101_IdTypeExp_TypeDoesntHaveTypeParams,
        T0102_IdTypeExp_VarTypeCantApplyTypeArgs,
        T0103_IdTypeExp_MultipleTypesOfSameName,
        T0104_IdTypeExp_TypeNotFound,
        T0105_IdTypeExp_TypeVarCantApplyTypeArgs,

        // T0201_MemberTypeExp_TypeIsNotNormalType,
        T0202_MemberTypeExp_MemberTypeNotFound,
        T0203_MemberTypeExp_ExpShouldNotBeType,
    }
}