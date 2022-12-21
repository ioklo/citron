using System;

using Citron.Infra;
using Citron.Collections;

using Citron.Symbol;

using S = Citron.Syntax;
using M = Citron.Module;

namespace Citron.Analysis
{
    static class BuilderMisc
    {
        public static ImmutableArray<M.Name> VisitTypeParams(ImmutableArray<S.TypeParam> typeParams)
        {
            var typeVarDeclsBuilder = ImmutableArray.CreateBuilder<M.Name>(typeParams.Length);

            foreach (var typeParam in typeParams)
            {
                var typeVarDecl = new M.Name.Normal(typeParam.Name);
                typeVarDeclsBuilder.Add(typeVarDecl);
            }

            return typeVarDeclsBuilder.MoveToImmutable();
        }

        public static ImmutableArray<FuncParameter> MakeParameters(IDeclSymbolNode curNode, BuildingFuncDeclPhaseContext context, ImmutableArray<S.FuncParam> sparams)
        {
            var builder = ImmutableArray.CreateBuilder<FuncParameter>(sparams.Length);
            foreach (var sparam in sparams)
            {
                var type = context.MakeTypeSymbol(curNode, sparam.Type);
                if (type == null) throw new NotImplementedException(); // 에러 처리

                var paramKind = sparam.Kind switch
                {
                    S.FuncParamKind.Normal => M.FuncParameterKind.Default,
                    S.FuncParamKind.Params => M.FuncParameterKind.Params,
                    S.FuncParamKind.Ref => M.FuncParameterKind.Ref,
                    _ => throw new UnreachableCodeException()
                };

                builder.Add(new FuncParameter(paramKind, type, new M.Name.Normal(sparam.Name)));
            }

            return builder.MoveToImmutable();
        }

        public static M.Accessor MakeGlobalMemberAccessor(S.AccessModifier? modifier)
        {
            return modifier switch
            {
                null => M.Accessor.Private,
                S.AccessModifier.Public => M.Accessor.Public,
                S.AccessModifier.Private => throw new NotImplementedException(), // 에러처리
                S.AccessModifier.Protected => throw new NotImplementedException(),
                _ => throw new UnreachableCodeException()
            };
        }

        public static M.Accessor MakeClassMemberAccessor(S.AccessModifier? accessModifier) // throws FatalException
        {
            return accessModifier switch
            {
                null => M.Accessor.Private,
                S.AccessModifier.Public => M.Accessor.Public,
                _ => throw new NotImplementedException() // 에러처리
            };
        }

        public static M.Accessor MakeStructMemberAccessor(S.AccessModifier? accessModifier) // throws FatalException
        {
            return accessModifier switch
            {
                null => M.Accessor.Public,
                S.AccessModifier.Private => M.Accessor.Private,
                S.AccessModifier.Public => throw new NotImplementedException(), // 에러처리
                S.AccessModifier.Protected => throw new NotImplementedException(), // 에러처리
                _ => throw new UnreachableCodeException()
            };
        }

    }
}