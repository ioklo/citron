using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using S = Gum.Syntax;
using M = Gum.CompileTime;

using static Gum.CompileTime.DeclSymbolIdExtensions;
using static Gum.CompileTime.SymbolIdExtensions;
using static Gum.CompileTime.SymbolPathExtensions;

namespace Citron.Analysis
{
    public enum TypeExpInfoKind
    {
        Class,
        Struct,
        Interface,
        Enum,
        EnumElem,
        Var,
        Void,
        TypeVar,
        Nullable,
    }

    // value
    // X<int>.Y<T>, closed
    public abstract class TypeExpInfo
    {
        public abstract TypeExpInfoKind GetKind();
        public abstract M.SymbolId GetSymbolId();
        // memberTypeExp: 리턴할 TypeExpInfo를 생성하는데 필요한 typeExp
        public abstract TypeExpInfo? GetMemberInfo(string name, ImmutableArray<M.SymbolId> typeArgs, S.TypeExp memberTypeExp); 
        public abstract S.TypeExp GetTypeExp();
    }   
    
    [AutoConstructor]
    partial class InternalTypeExpInfo : TypeExpInfo
    {
        M.ModuleSymbolId typeId;
        TypeSkeleton skeleton; // 멤버를 갖고 오기 위한 수단
        S.TypeExp typeExp;

        public override TypeExpInfoKind GetKind()
        {
            return skeleton.Kind.GetTypeExpInfoKind();
        }

        public override M.SymbolId GetSymbolId()
        {
            return typeId;
        }

        public override TypeExpInfo? GetMemberInfo(string name, ImmutableArray<M.SymbolId> typeArgs, S.TypeExp memberTypeExp)
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
    partial class ModuleSymbolTypeExpInfo : TypeExpInfo
    {
        M.ModuleSymbolId typeId;
        ITypeDeclSymbol symbol;
        S.TypeExp typeExp; // 소스의 어디에서 이 타입정보가 나타나는가

        public override TypeExpInfoKind GetKind()
        {
            return symbol.GetTypeExpInfoKind();
        }

        public override TypeExpInfo? GetMemberInfo(string memberName, ImmutableArray<M.SymbolId> typeArgs, S.TypeExp memberTypeExp)
        {
            var memberTypeNode = symbol.GetMemberDeclNode(new M.Name.Normal(memberName), typeArgs.Length, default) as ITypeDeclSymbol;
            if (memberTypeNode == null)
                return null;

            var memberTypeId = typeId.Child(new M.Name.Normal(memberName), typeArgs);
            return new ModuleSymbolTypeExpInfo(memberTypeId, memberTypeNode, memberTypeExp);
        }

        public override M.SymbolId GetSymbolId()
        {
            return typeId;
        }

        public override S.TypeExp GetTypeExp()
        {
            return typeExp;
        }
    }

    public class InternalTypeVarTypeExpInfo : TypeExpInfo
    {
        M.TypeVarSymbolId id;
        S.TypeExp typeExp;

        InternalTypeVarTypeExpInfo(M.TypeVarSymbolId id, S.TypeExp typeExp)
        {
            this.id = id;
            this.typeExp = typeExp;
        }

        public override TypeExpInfoKind GetKind()
        {
            return TypeExpInfoKind.TypeVar;
        }

        public override TypeExpInfo? GetMemberInfo(string name, ImmutableArray<M.SymbolId> typeArgs, S.TypeExp memberTypeExp)
        {
            return null;
        }
        
        public override S.TypeExp GetTypeExp()
        {
            return typeExp;
        }

        public override M.SymbolId GetSymbolId()
        {
            return id;
        }

        public static InternalTypeVarTypeExpInfo Make(M.DeclSymbolId outerDeclId, string typeParam, int index, S.TypeExp typeExp)
        {            
            var declId = outerDeclId.Child(new M.Name.Normal(typeParam), 0, default);
            return new InternalTypeVarTypeExpInfo(new M.TypeVarSymbolId(declId, index), typeExp);
        }
    }

    class NoMemberTypeExpInfo : TypeExpInfo
    {
        TypeExpInfoKind kind;
        M.SymbolId id;
        S.TypeExp typeExp;

        public NoMemberTypeExpInfo(TypeExpInfoKind kind, M.SymbolId id, S.TypeExp typeExp)
        {
            this.kind = kind;
            this.id = id;
            this.typeExp = typeExp;
        }

        public override TypeExpInfoKind GetKind()
        {
            return kind;
        }

        public override TypeExpInfo? GetMemberInfo(string name, ImmutableArray<M.SymbolId> typeArgs, S.TypeExp memberTypeExp)
        {
            return null;
        }

        public override M.SymbolId GetSymbolId()
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
        public static TypeExpInfo Var(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(TypeExpInfoKind.Var, new M.VarSymbolId(), typeExp);
        }

        public static TypeExpInfo Void(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(TypeExpInfoKind.Void, new M.VoidSymbolId(), typeExp);
        }

        public static TypeExpInfo Nullable(M.SymbolId innerTypeId, S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(TypeExpInfoKind.Nullable, new M.NullableSymbolId(innerTypeId), typeExp);
        }
    }

    // int, bool, ...
    // 아직 RuntimeModule로 빼지 못한 것들
    public static class BuiltinTypeExpInfo
    {
        static M.Name.Normal runtimeModule;
        static M.SymbolPath systemPath;

        static BuiltinTypeExpInfo()
        {
            runtimeModule = new M.Name.Normal("System.Runtime");
            systemPath = new M.SymbolPath(null, new M.Name.Normal("System"));
        }

        public static TypeExpInfo Bool(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(TypeExpInfoKind.Struct, new M.ModuleSymbolId(runtimeModule, systemPath.Child(new M.Name.Normal("Int32"))), typeExp);
        }

        public static TypeExpInfo Int(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(TypeExpInfoKind.Struct, new M.ModuleSymbolId(runtimeModule, systemPath.Child(new M.Name.Normal("Boolean"))), typeExp);
        }

        public static TypeExpInfo String(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(TypeExpInfoKind.Struct, new M.ModuleSymbolId(runtimeModule, systemPath.Child(new M.Name.Normal("String"))), typeExp);
        }        
    }
}
