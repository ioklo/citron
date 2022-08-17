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

namespace Citron.Analysis
{   
    [AutoConstructor]
    partial class InternalTypeExpInfo : S.TypeExpInfo
    {
        ModuleSymbolId typeId;
        TypeSkeleton skeleton; // 멤버를 갖고 오기 위한 수단
        S.TypeExp typeExp;

        public override S.TypeExpInfoKind GetKind()
        {
            return skeleton.Kind.GetTypeExpInfoKind();
        }

        public override SymbolId GetSymbolId()
        {
            return typeId;
        }

        public override S.TypeExpInfo? GetMemberInfo(string name, ImmutableArray<SymbolId> typeArgs, S.TypeExp memberTypeExp)
        {
            var mname = new M.Name.Normal(name);

            var memberSkeleton = skeleton.GetMember(mname, typeArgs.Length);
            if (memberSkeleton == null) return null;

            var memberId = typeId.Child(mname, typeArgs, default);
            return new InternalTypeExpInfo(memberId, memberSkeleton, memberTypeExp);
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
        ITypeDeclSymbol symbol;
        S.TypeExp typeExp; // 소스의 어디에서 이 타입정보가 나타나는가

        public override S.TypeExpInfoKind GetKind()
        {
            return symbol.GetTypeExpInfoKind();
        }

        public override S.TypeExpInfo? GetMemberInfo(string memberName, ImmutableArray<SymbolId> typeArgs, S.TypeExp memberTypeExp)
        {
            var memberTypeNode = symbol.GetMemberDeclNode(new DeclSymbolNodeName(new M.Name.Normal(memberName), typeArgs.Length, default)) as ITypeDeclSymbol;
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

    public class InternalTypeVarTypeExpInfo : S.TypeExpInfo
    {
        TypeVarSymbolId id;
        S.TypeExp typeExp;

        InternalTypeVarTypeExpInfo(TypeVarSymbolId id, S.TypeExp typeExp)
        {
            this.id = id;
            this.typeExp = typeExp;
        }

        public override S.TypeExpInfoKind GetKind()
        {
            return S.TypeExpInfoKind.TypeVar;
        }

        public override S.TypeExpInfo? GetMemberInfo(string name, ImmutableArray<SymbolId> typeArgs, S.TypeExp memberTypeExp)
        {
            return null;
        }
        
        public override S.TypeExp GetTypeExp()
        {
            return typeExp;
        }

        public override SymbolId GetSymbolId()
        {
            return id;
        }

        public static InternalTypeVarTypeExpInfo Make(DeclSymbolId outerDeclId, string typeParam, int index, S.TypeExp typeExp)
        {            
            var declId = outerDeclId.Child(new M.Name.Normal(typeParam), 0, default);
            return new InternalTypeVarTypeExpInfo(new TypeVarSymbolId(declId, index), typeExp);
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

        public override S.TypeExpInfo? GetMemberInfo(string name, ImmutableArray<SymbolId> typeArgs, S.TypeExp memberTypeExp)
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
