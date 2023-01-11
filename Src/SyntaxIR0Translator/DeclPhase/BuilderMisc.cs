using System;

using Citron.Infra;
using Citron.Collections;

using Citron.Symbol;

using S = Citron.Syntax;

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

        public static ImmutableArray<FuncParameter> MakeParameters(IDeclSymbolNode curNode, BuildingFuncDeclPhaseContext context, ImmutableArray<S.FuncParam> sparams)
        {
            var builder = ImmutableArray.CreateBuilder<FuncParameter>(sparams.Length);
            foreach (var sparam in sparams)
            {
                var type = context.MakeType(sparam.Type, curNode);
                if (type == null) throw new NotImplementedException(); // 에러 처리

                var paramKind = sparam.Kind switch
                {
                    S.FuncParamKind.Normal => FuncParameterKind.Default,
                    S.FuncParamKind.Params => FuncParameterKind.Params,
                    S.FuncParamKind.Ref => FuncParameterKind.Ref,
                    _ => throw new UnreachableCodeException()
                };

                builder.Add(new FuncParameter(paramKind, type, new Name.Normal(sparam.Name)));
            }

            return builder.MoveToImmutable();
        }

        public static Accessor MakeGlobalMemberAccessor(S.AccessModifier? modifier)
        {
            return modifier switch
            {
                null => Accessor.Private,
                S.AccessModifier.Public => Accessor.Public,
                S.AccessModifier.Private => throw new NotImplementedException(), // 에러처리
                S.AccessModifier.Protected => throw new NotImplementedException(),
                _ => throw new UnreachableCodeException()
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
                _ => throw new UnreachableCodeException()
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