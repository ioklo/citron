using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

using S = Gum.Syntax;
using M = Gum.CompileTime;

namespace Gum.IR0
{
    // 종류
    // InternalGlobalFuncValue - 내가 모듈이다, FuncDecl
    // InternalMemberFuncValue - InternalTypeValue outer, FuncDecl 참조
    // ExternalGlobalFuncValue - 모듈을 찾아야 하기 때문에, ModuleName, NamespacePath
    // ExternalMemberFuncValue - ExternalTypeValue outer;
    // => (internal/external) (root/member)

    // X<int>.Y<short>.F_T_int_int<S>
    class FuncValue : ItemValue
    {
        // X<int>.Y<short>
        M.ModuleName? moduleName;       // external root일 경우에만 존재
        M.NamespacePath? namespacePath; // (internal/external) root일 경우에만 존재
        TypeValue? outer;               // (internal/external) member일 경우에만 존재
        FuncDeclId? funcDeclId;         // internal (root/member)일 경우에만 존재        

        // F_int_int
        M.FuncInfo funcInfo;

        ImmutableArray<TypeValue> typeArgs;

        public bool IsStatic { get => !funcInfo.IsInstanceFunc; }
        public bool IsSequence { get => funcInfo.IsSequenceFunc; }

        // external root
        public FuncValue(M.ModuleName moduleName, M.NamespacePath namespacePath, M.FuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
            : this(moduleName, namespacePath, null, null, funcInfo, typeArgs) { }

        // external member
        public FuncValue(TypeValue outer, M.FuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
            : this(null, null, outer, null, funcInfo, typeArgs) { }

        // internal root
        public FuncValue(M.NamespacePath namespacePath, FuncDeclId funcDeclId, M.FuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
            : this(null, namespacePath, null, funcDeclId, funcInfo, typeArgs) { }

        // internal member
        public FuncValue(TypeValue outer, FuncDeclId funcDeclId, M.FuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs) 
            : this(null, null, outer, funcDeclId, funcInfo, typeArgs) { }

        FuncValue(M.ModuleName? moduleName, M.NamespacePath? namespacePath, TypeValue? outer, FuncDeclId? funcDeclId, M.FuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
        {
            this.moduleName = moduleName;
            this.namespacePath = namespacePath;
            this.outer = outer;
            this.funcDeclId = funcDeclId;
            this.funcInfo = funcInfo;
            this.typeArgs = typeArgs;
        }

                

    }
}
