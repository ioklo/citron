using System;

using Citron.Infra;
using Citron.Collections;

using Citron.Symbol;

using S = Citron.Syntax;
using System.Diagnostics;

namespace Citron.Analysis
{
    static class BuilderMisc
    {
        public static ImmutableArray<Name> VisitTypeParams(ImmutableArray<S.TypeParam> typeParams)
        {
            var typeVarDeclsBuilder = ImmutableArray.CreateBuilder<Name>(typeParams.Length);

            foreach (var typeParam in typeParams)
            {
                var typeVarDecl = new Name.Normal(typeParam.Name);
                typeVarDeclsBuilder.Add(typeVarDecl);
            }

            return typeVarDeclsBuilder.MoveToImmutable();
        }

        public static (ImmutableArray<FuncParameter> Params, bool bLastParamVariadic) MakeParameters(IDeclSymbolNode curNode, BuildingMemberDeclPhaseContext context, ImmutableArray<S.FuncParam> sparams)
        {
            bool bLastParamVariadic = false;

            int paramsCount = sparams.Length;
            var builder = ImmutableArray.CreateBuilder<FuncParameter>(paramsCount);

            for(int i = 0; i < paramsCount; i++)            
            {
                var sparam = sparams[i];

                var type = context.MakeType(sparam.Type, curNode);
                if (type == null) throw new NotImplementedException(); // 에러 처리

                if (sparam.HasParams)
                {
                    if (i == paramsCount - 1)
                    {
                        bLastParamVariadic = true;
                    }
                    else
                    {
                        throw new NotImplementedException(); // 에러 처리, params는 마지막 파라미터에만 사용할 수 있습니다
                    }

                }

                builder.Add(new FuncParameter(type, new Name.Normal(sparam.Name)));
            }

            return (builder.MoveToImmutable(), bLastParamVariadic);
        }

        public static Accessor MakeGlobalMemberAccessor(S.AccessModifier? modifier)
        {
            return modifier switch
            {
                null => Accessor.Private,
                S.AccessModifier.Public => Accessor.Public,
                S.AccessModifier.Private => throw new NotImplementedException(), // 에러처리
                S.AccessModifier.Protected => throw new NotImplementedException(),
                _ => throw new UnreachableException()
            };
        }

        public static Accessor MakeClassMemberAccessor(S.AccessModifier? accessModifier) // throws FatalException
        {
            return accessModifier switch
            {
                null => Accessor.Private,
                S.AccessModifier.Public => Accessor.Public,
                _ => throw new NotImplementedException() // 에러처리
            };
        }

        public static Accessor MakeStructMemberAccessor(S.AccessModifier? accessModifier) // throws FatalException
        {
            return accessModifier switch
            {
                null => Accessor.Public,
                S.AccessModifier.Private => Accessor.Private,
                S.AccessModifier.Public => throw new NotImplementedException(), // 에러처리
                S.AccessModifier.Protected => throw new NotImplementedException(), // 에러처리
                _ => throw new UnreachableException()
            };
        }

        public static Name.ConstructorParam MakeBaseConstructorParamName(int index, Name baseParamName)
        {
            if (baseParamName is Name.ConstructorParam specialName)
            {
                return new Name.ConstructorParam(index, specialName.Text);
            }
            else if (baseParamName is Name.Normal normalName)
            {
                return new Name.ConstructorParam(index, normalName.Text);
            }
            else
            {
                throw new RuntimeFatalException();
            }
        }
    }
}