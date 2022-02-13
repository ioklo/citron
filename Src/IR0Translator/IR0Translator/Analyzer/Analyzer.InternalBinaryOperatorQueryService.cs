using System.Collections.Generic;
using System.Linq;
using Citron.Collections;

using static Citron.Infra.Misc;

using S = Citron.Syntax;
using R = Citron.IR0;
using System;
using Citron.Infra;
using Citron.Analysis;

namespace Citron.IR0Translator
{
    partial class Analyzer
    {
        struct InternalBinaryOperatorInfo
        {
            public ITypeSymbol OperandType0 { get; }
            public ITypeSymbol OperandType1 { get; }
            public ITypeSymbol ResultType { get; }
            public R.InternalBinaryOperator IR0Operator { get; }

            public InternalBinaryOperatorInfo(
                ITypeSymbol operandType0,
                ITypeSymbol operandType1,
                ITypeSymbol resultType,
                R.InternalBinaryOperator ir0Operator)
            {
                OperandType0 = operandType0;
                OperandType1 = operandType1;
                ResultType = resultType;
                IR0Operator = ir0Operator;
            }
        }

        struct InternalBinaryOperatorQueryService
        {
            Dictionary<S.BinaryOpKind, ImmutableArray<InternalBinaryOperatorInfo>> infos;

            public InternalBinaryOperatorQueryService(ITypeSymbol boolType, ITypeSymbol intType, ITypeSymbol stringType)
            {
                infos = new Dictionary<S.BinaryOpKind, ImmutableArray<InternalBinaryOperatorInfo>>();                

                infos = new Dictionary<S.BinaryOpKind, ImmutableArray<InternalBinaryOperatorInfo>>()
                {
                    { S.BinaryOpKind.Multiply, Arr(new InternalBinaryOperatorInfo(intType, intType, intType, R.InternalBinaryOperator.Multiply_Int_Int_Int) ) },
                    { S.BinaryOpKind.Divide, Arr(new InternalBinaryOperatorInfo(intType, intType, intType, R.InternalBinaryOperator.Divide_Int_Int_Int) ) },
                    { S.BinaryOpKind.Modulo, Arr(new InternalBinaryOperatorInfo(intType, intType, intType, R.InternalBinaryOperator.Modulo_Int_Int_Int) ) },
                    { S.BinaryOpKind.Add,  Arr(new InternalBinaryOperatorInfo(intType, intType, intType, R.InternalBinaryOperator.Add_Int_Int_Int),
                        new InternalBinaryOperatorInfo(stringType, stringType, stringType, R.InternalBinaryOperator.Add_String_String_String) ) },

                    { S.BinaryOpKind.Subtract, Arr(new InternalBinaryOperatorInfo(intType, intType, intType, R.InternalBinaryOperator.Subtract_Int_Int_Int) ) },

                    { S.BinaryOpKind.LessThan, Arr(new InternalBinaryOperatorInfo(intType, intType, boolType, R.InternalBinaryOperator.LessThan_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, R.InternalBinaryOperator.LessThan_String_String_Bool) ) },

                    { S.BinaryOpKind.GreaterThan, Arr(new InternalBinaryOperatorInfo(intType, intType, boolType, R.InternalBinaryOperator.GreaterThan_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, R.InternalBinaryOperator.GreaterThan_String_String_Bool) ) },

                    { S.BinaryOpKind.LessThanOrEqual, Arr(new InternalBinaryOperatorInfo(intType, intType, boolType, R.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, R.InternalBinaryOperator.LessThanOrEqual_String_String_Bool) ) },

                    { S.BinaryOpKind.GreaterThanOrEqual, Arr(new InternalBinaryOperatorInfo(intType, intType, boolType, R.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, R.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool) ) },

                    { S.BinaryOpKind.Equal, Arr(new InternalBinaryOperatorInfo(intType, intType, boolType, R.InternalBinaryOperator.Equal_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(boolType, boolType, boolType, R.InternalBinaryOperator.Equal_Bool_Bool_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, R.InternalBinaryOperator.Equal_String_String_Bool) ) },
                };
            }

            public ImmutableArray<InternalBinaryOperatorInfo> GetInfos(S.BinaryOpKind kind)
            {
                if (infos.TryGetValue(kind, out var result))
                    return result;

                return default;
            }
        }
    }
}
