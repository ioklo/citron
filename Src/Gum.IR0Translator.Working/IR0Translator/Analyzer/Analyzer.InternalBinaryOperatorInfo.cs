using System.Collections.Generic;

using S = Gum.Syntax;
using R = Gum.IR0;
using System.Linq;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        struct InternalBinaryOperatorInfo
        {
            public TypeValue OperandType0 { get; }
            public TypeValue OperandType1 { get; }
            public TypeValue ResultType { get; }
            public R.InternalBinaryOperator IR0Operator { get; }

            public InternalBinaryOperatorInfo(
                TypeValue operandType0,
                TypeValue operandType1,
                TypeValue resultType,
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
            Dictionary<S.BinaryOpKind, InternalBinaryOperatorInfo[]> infos;

            public InternalBinaryOperatorQueryService(ItemValueFactory itemValueFactory)
            {
                infos = new Dictionary<S.BinaryOpKind, InternalBinaryOperatorInfo[]>();

                var boolType = itemValueFactory.Bool;
                var intType = itemValueFactory.Int;
                var stringType = itemValueFactory.String;

                infos = new Dictionary<S.BinaryOpKind, InternalBinaryOperatorInfo[]>()
                {
                    { S.BinaryOpKind.Multiply, new[]{ new InternalBinaryOperatorInfo(intType, intType, intType, R.InternalBinaryOperator.Multiply_Int_Int_Int) } },
                    { S.BinaryOpKind.Divide, new[]{ new InternalBinaryOperatorInfo(intType, intType, intType, R.InternalBinaryOperator.Divide_Int_Int_Int) } },
                    { S.BinaryOpKind.Modulo, new[]{ new InternalBinaryOperatorInfo(intType, intType, intType, R.InternalBinaryOperator.Modulo_Int_Int_Int) } },
                    { S.BinaryOpKind.Add,  new[]{
                        new InternalBinaryOperatorInfo(intType, intType, intType, R.InternalBinaryOperator.Add_Int_Int_Int),
                        new InternalBinaryOperatorInfo(stringType, stringType, stringType, R.InternalBinaryOperator.Add_String_String_String) } },

                    { S.BinaryOpKind.Subtract, new[]{ new InternalBinaryOperatorInfo(intType, intType, intType, R.InternalBinaryOperator.Subtract_Int_Int_Int) } },

                    { S.BinaryOpKind.LessThan, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, R.InternalBinaryOperator.LessThan_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, R.InternalBinaryOperator.LessThan_String_String_Bool) } },

                    { S.BinaryOpKind.GreaterThan, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, R.InternalBinaryOperator.GreaterThan_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, R.InternalBinaryOperator.GreaterThan_String_String_Bool) } },

                    { S.BinaryOpKind.LessThanOrEqual, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, R.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, R.InternalBinaryOperator.LessThanOrEqual_String_String_Bool) } },

                    { S.BinaryOpKind.GreaterThanOrEqual, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, R.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, R.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool) } },

                    { S.BinaryOpKind.Equal, new[]{
                        new InternalBinaryOperatorInfo(intType, intType, boolType, R.InternalBinaryOperator.Equal_Int_Int_Bool),
                        new InternalBinaryOperatorInfo(boolType, boolType, boolType, R.InternalBinaryOperator.Equal_Bool_Bool_Bool),
                        new InternalBinaryOperatorInfo(stringType, stringType, boolType, R.InternalBinaryOperator.Equal_String_String_Bool) } },
                };
            }

            public IEnumerable<InternalBinaryOperatorInfo> GetInfos(S.BinaryOpKind kind)
            {
                if (infos.TryGetValue(kind, out var result))
                    return result;

                return Enumerable.Empty<InternalBinaryOperatorInfo>();
            }
        }
    }
}
