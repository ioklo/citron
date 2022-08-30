using Citron.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using S = Citron.Syntax;
using M = Citron.Module;

using static Citron.Symbol.DeclSymbolIdExtensions;
using static Citron.Symbol.SymbolIdExtensions;
using static Citron.Symbol.SymbolPathExtensions;
using Citron.Symbol;
using Citron.Infra;

namespace Citron.Analysis
{   
    [AutoConstructor]
    partial class InternalTypeExpInfo : S.TypeExpInfo
    {   
        Skeleton skeleton; // 멤버를 갖고 오기 위한 수단
        ImmutableArray<SymbolId> typeArgs;
        S.TypeExp typeExp;

        public override S.TypeExpInfoKind GetKind()
        {
            return skeleton.Kind.GetTypeExpInfoKind();
        }

        public override SymbolId GetSymbolId()
        {
            // 이걸 어떻게 만들 것인가
            return typeId;
        }

        // 이 시점에 typeArgs가 유효한가?
        public override S.TypeExpInfo? MakeMemberInfo(string name, ImmutableArray<SymbolId> typeArgs, S.TypeExp memberTypeExp)
        {
            var mname = new M.Name.Normal(name);

            var result = skeleton.GetUniqueMember(mname, typeArgs.Length);
            switch(result)
            {
                case UniqueQueryResult<Skeleton>.Found foundResult:
                    return new InternalTypeExpInfo(foundResult.Value, typeArgs, memberTypeExp);

                case UniqueQueryResult<Skeleton>.MultipleError:
                    return null;

                case UniqueQueryResult<Skeleton>.NotFound:
                    return null;

                default:
                    throw new UnreachableCodeException();
            }
        }
        
        public override S.TypeExp GetTypeExp()
        {
            return typeExp;
        }
    }

    [AutoConstructor]
    partial class ModuleSymbolTypeExpInfo : S.TypeExpInfo
    {
        ModuleSymbolId typeId;
        ITypeDeclSymbol declSymbol;
        S.TypeExp typeExp; // 소스의 어디에서 이 타입정보가 나타나는가

        public override S.TypeExpInfoKind GetKind()
        {
            return declSymbol.GetTypeExpInfoKind();
        }

        public override S.TypeExpInfo? MakeMemberInfo(string memberName, ImmutableArray<SymbolId> typeArgs, S.TypeExp memberTypeExp)
        {
            var memberTypeNode = declSymbol.GetMemberDeclNode(new DeclSymbolNodeName(new M.Name.Normal(memberName), typeArgs.Length, default)) as ITypeDeclSymbol;
            if (memberTypeNode == null)
                return null;

            var memberTypeId = typeId.Child(new M.Name.Normal(memberName), typeArgs);
            return new ModuleSymbolTypeExpInfo(memberTypeId, memberTypeNode, memberTypeExp);
        }

        public override SymbolId GetSymbolId()
        {
            return typeId;
        }

        public override S.TypeExp GetTypeExp()
        {
            return typeExp;
        }
    }

    class NoMemberTypeExpInfo : S.TypeExpInfo
    {
        S.TypeExpInfoKind kind;
        SymbolId id;
        S.TypeExp typeExp;

        public NoMemberTypeExpInfo(S.TypeExpInfoKind kind, SymbolId id, S.TypeExp typeExp)
        {
            this.kind = kind;
            this.id = id;
            this.typeExp = typeExp;
        }

        public override S.TypeExpInfoKind GetKind()
        {
            return kind;
        }

        public override S.TypeExpInfo? MakeMemberInfo(string name, ImmutableArray<SymbolId> typeArgs, S.TypeExp memberTypeExp)
        {
            return null;
        }

        public override SymbolId GetSymbolId()
        {
            return id;
        }
        
        public override S.TypeExp GetTypeExp()
        {
            return typeExp;
        }
    }

    // 스페셜한 타입정보
    public static class SpecialTypeExpInfo
    {
        public static S.TypeExpInfo Var(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(S.TypeExpInfoKind.Var, new VarSymbolId(), typeExp);
        }

        public static S.TypeExpInfo Void(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(S.TypeExpInfoKind.Void, new VoidSymbolId(), typeExp);
        }

        public static S.TypeExpInfo Nullable(SymbolId innerTypeId, S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(S.TypeExpInfoKind.Nullable, new NullableSymbolId(innerTypeId), typeExp);
        }
    }

    // int, bool, ...
    // 아직 RuntimeModule로 빼지 못한 것들
    public static class BuiltinTypeExpInfo
    {
        static M.Name.Normal runtimeModule;
        static SymbolPath systemPath;

        static BuiltinTypeExpInfo()
        {
            runtimeModule = new M.Name.Normal("System.Runtime");
            systemPath = new SymbolPath(null, new M.Name.Normal("System"));
        }

        public static S.TypeExpInfo Bool(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(S.TypeExpInfoKind.Struct, new ModuleSymbolId(runtimeModule, systemPath.Child(new M.Name.Normal("Boolean"))), typeExp);
        }

        public static S.TypeExpInfo Int(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(S.TypeExpInfoKind.Struct, new ModuleSymbolId(runtimeModule, systemPath.Child(new M.Name.Normal("Int32"))), typeExp);
        }

        public static S.TypeExpInfo String(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(S.TypeExpInfoKind.Struct, new ModuleSymbolId(runtimeModule, systemPath.Child(new M.Name.Normal("String"))), typeExp);
        }        
    }
}
