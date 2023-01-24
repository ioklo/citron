using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citron.Collections;
using Citron.Symbol;
using S = Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis
{
    internal static class BodyMisc
    {
        public static ImmutableArray<IType> MakeTypeArgs(ImmutableArray<S.TypeExp> typeArgsSyntax, ScopeContext context)
        {
            var builder = ImmutableArray.CreateBuilder<IType>(typeArgsSyntax.Length);

            foreach (var typeExp in typeArgsSyntax)
            {
                var typeValue = context.MakeType(typeExp);
                builder.Add(typeValue);
            }

            return builder.MoveToImmutable();
        }

        public static R.Exp? TryCastExp_Exp(R.Exp exp, IType expectedType) // nothrow
        {
            var expType = exp.GetExpType();

            // 같으면 그대로 리턴
            if (expectedType.Equals(expType))
                return exp;

            // TODO: TypeValue에 TryCast를 각각 넣기
            // expectType.TryCast(exp); // expResult를 넣는것도 이상하다.. 그건 그때가서

            // 1. enumElem -> enum
            if (expType is EnumElemSymbol enumElem)
            {
                if (expectedType is EnumSymbol expectEnumType)
                {
                    if (expectedType.Equals(enumElem.GetOuter()))
                    {
                        return new R.CastEnumElemToEnumExp(exp, enumElem);
                    }
                }

                return null;
            }

            // 2. exp is class type
            if (expType is ClassSymbol @class)
            {
                if (expectedType is ClassSymbol expectedClass)
                {
                    // allows upcast
                    if (expectedClass.IsBaseOf(@class))
                    {
                        return new R.CastClassExp(exp, expectedClass);
                    }

                    return null;
                }

                // TODO: interface
                // if (expectType is InterfaceTypeValue )
            }

            // TODO: 3. C -> Nullable<C>, C -> B -> Nullable<B> 허용
            //if (IsNullableType(expectedType, out var expectedInnerType))
            //{
            //    // C -> B 시도
            //    var castToInnerTypeExp = TryCastExp_Exp(exp, expectedInnerType);
            //    if (castToInnerTypeExp != null)
            //    {
            //        // B -> B?
            //        return MakeNullableExp(castToInnerTypeExp);
            //        return new R.NewNullableExp(castToInnerTypeExp, expectedNullableType);
            //    }
            //}

            return null;
        }
    }
}
