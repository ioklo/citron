using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using S = Gum.Syntax;
using M = Gum.CompileTime;

namespace Gum.Analysis
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
        public abstract SymbolId GetSymbolId();
        // memberTypeExp: 리턴할 TypeExpInfo를 생성하는데 필요한 typeExp
        public abstract TypeExpInfo? GetMemberInfo(string name, ImmutableArray<SymbolId> typeArgs, S.TypeExp memberTypeExp); 
        public abstract S.TypeExp GetTypeExp();
    }   
    
    [AutoConstructor]
    partial class InternalTypeExpInfo : TypeExpInfo
    {
        ModuleSymbolId typeId;
        TypeSkeleton skeleton; // 멤버를 갖고 오기 위한 수단
        S.TypeExp typeExp;

        public override TypeExpInfoKind GetKind()
        {
            return skeleton.Kind.GetTypeExpInfoKind();
        }

        public override SymbolId GetSymbolId()
        {
            return typeId;
        }

        public override TypeExpInfo? GetMemberInfo(string name, ImmutableArray<SymbolId> typeArgs, S.TypeExp memberTypeExp)
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
        ModuleSymbolId typeId;
        ITypeDeclSymbol symbol;
        S.TypeExp typeExp; // 소스의 어디에서 이 타입정보가 나타나는가

        public override TypeExpInfoKind GetKind()
        {
            return symbol.GetTypeExpInfoKind();
        }

        public override TypeExpInfo? GetMemberInfo(string memberName, ImmutableArray<SymbolId> typeArgs, S.TypeExp memberTypeExp)
        {
            var memberTypeNode = symbol.GetMemberDeclNode(new M.Name.Normal(memberName), typeArgs.Length, default) as ITypeDeclSymbol;
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

    public class InternalTypeVarTypeExpInfo : TypeExpInfo
    {
        TypeVarSymbolId id;
        int index;
        S.TypeExp typeExp;

        InternalTypeVarTypeExpInfo(TypeVarSymbolId id, int index, S.TypeExp typeExp)
        {
            this.id = id;
            this.index = index;
            this.typeExp = typeExp;
        }

        public override TypeExpInfoKind GetKind()
        {
            return TypeExpInfoKind.TypeVar;
        }

        public override TypeExpInfo? GetMemberInfo(string name, ImmutableArray<SymbolId> typeArgs, S.TypeExp memberTypeExp)
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
            return new InternalTypeVarTypeExpInfo(new TypeVarSymbolId(declId), index, typeExp);
        }
    }

    class NoMemberTypeExpInfo : TypeExpInfo
    {
        TypeExpInfoKind kind;
        SymbolId? id;
        S.TypeExp typeExp;

        public NoMemberTypeExpInfo(TypeExpInfoKind kind, SymbolId? id, S.TypeExp typeExp)
        {
            this.kind = kind;
            this.id = id;
            this.typeExp = typeExp;
        }

        public override TypeExpInfoKind GetKind()
        {
            return kind;
        }

        public override TypeExpInfo? GetMemberInfo(string name, ImmutableArray<SymbolId> typeArgs, S.TypeExp memberTypeExp)
        {
            return null;
        }

        public override SymbolId GetSymbolId()
        {            
            Debug.Assert(id != null); // null을 실제로 사용할 경우를 알아보기 위해 일부러 넣음
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
            return new NoMemberTypeExpInfo(TypeExpInfoKind.Var, null, typeExp);
        }

        public static TypeExpInfo Void(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(TypeExpInfoKind.Void, new VoidSymbolId(), typeExp);
        }

        public static TypeExpInfo Nullable(SymbolId innerTypeId, S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(TypeExpInfoKind.Nullable, new NullableSymbolId(innerTypeId), typeExp);
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

        public static TypeExpInfo Bool(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(TypeExpInfoKind.Struct, new ModuleSymbolId(runtimeModule, systemPath.Child(new M.Name.Normal("Int32"))), typeExp);
        }

        public static TypeExpInfo Int(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(TypeExpInfoKind.Struct, new ModuleSymbolId(runtimeModule, systemPath.Child(new M.Name.Normal("Boolean"))), typeExp);
        }

        public static TypeExpInfo String(S.TypeExp typeExp)
        {
            return new NoMemberTypeExpInfo(TypeExpInfoKind.Struct, new ModuleSymbolId(runtimeModule, systemPath.Child(new M.Name.Normal("String"))), typeExp);
        }        
    }
}
