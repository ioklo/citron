
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using M = Gum.CompileTime;

namespace Gum.IR0
{
    abstract partial class TypeValue
    {
        
    }

    // "var"
    class VarTypeValue : TypeValue
    {
        public static VarTypeValue Instance { get; } = new VarTypeValue();
        private VarTypeValue() { }
    }

    // T: depth는 지역적이므로, 주어진 컨텍스트 안에서만 의미가 있다
    class TypeVarTypeValue : TypeValue
    {
        public int Depth { get; }
        public int Index { get; }
        public string Name { get; }

        public TypeVarTypeValue(int depth, int index, string name)
        {
            Depth = depth;
            Name = name;
        }
    }

    class StructValue : TypeValue
    {
        M.StructInfo structInfo;
        ImmutableArray<TypeValue> typeArgs;
        public MemberVarValue GetMemberVar();
    }    
    
    // (struct, class, enum) (external/internal) (global/member) type

    class NormalTypeValue : TypeValue
    {
        M.ModuleName? moduleName;       // external global
        M.NamespacePath? namespacePath; // (external/internal) global
        NormalTypeValue? outer;         // (external/internal) member
        TypeDeclId? typeDeclId;         // internal (global/member)

        M.TypeInfo typeInfo;
        ImmutableArray<TypeValue> typeArgs;

        public static NormalTypeValue MakeExternalGlobal(M.ModuleName moduleName, M.NamespacePath namespacePath, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
            => new NormalTypeValue(moduleName, namespacePath, null, null, typeInfo, typeArgs);

        public static NormalTypeValue MakeExternalMember(NormalTypeValue outer, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
            => new NormalTypeValue(null, null, outer, null, typeInfo, typeArgs);

        public static NormalTypeValue MakeInternalGlobal(M.NamespacePath namespacePath, TypeDeclId typeDeclId, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
            => new NormalTypeValue(null, namespacePath, null, typeDeclId, typeInfo, typeArgs);

        public static NormalTypeValue MakeInternalMember(NormalTypeValue outer, TypeDeclId typeDeclId, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
            => new NormalTypeValue(null, null, outer, typeDeclId, typeInfo, typeArgs);

        NormalTypeValue(M.ModuleName? moduleName, M.NamespacePath? namespacePath, NormalTypeValue? outer, TypeDeclId? typeDeclId, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
        {
            this.moduleName = moduleName;
            this.namespacePath = namespacePath;
            this.outer = outer;
            this.typeDeclId = typeDeclId;
            this.typeInfo = typeInfo;
            this.typeArgs = typeArgs;
        }
    }

    // "void"
    class VoidTypeValue : TypeValue
    {
        public static VoidTypeValue Instance { get; } = new VoidTypeValue();
        private VoidTypeValue() { }
    }

    // ArgTypeValues => RetValueTypes
    class FuncTypeValue : TypeValue
    {
        public TypeValue Return { get; }
        public ImmutableArray<TypeValue> Params { get; }

        public FuncTypeValue(TypeValue ret, IEnumerable<TypeValue> parameters)
        {
            Return = ret;
            Params = parameters.ToImmutableArray();
        }
    }

    // S.First, S.Second(int i, short s)
    class EnumElemTypeValue : TypeValue
    {
        public NormalTypeValue EnumTypeValue { get; }
        public string Name { get; }

        public EnumElemTypeValue(NormalTypeValue enumTypeValue, string name)
        {
            EnumTypeValue = enumTypeValue;
            Name = name;
        }
    }
}
