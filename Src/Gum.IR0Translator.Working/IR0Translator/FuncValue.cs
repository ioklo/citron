using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
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
        TypeValueFactory typeValueFactory;

        // X<int>.Y<short>
        M.ModuleName? moduleName;       // external global일 경우에만 존재
        M.NamespacePath? namespacePath; // (internal/external) global일 경우에만 존재
        TypeValue? outer;               // (internal/external) member일 경우에만 존재
        FuncDeclId? funcDeclId;         // internal (global/member)일 경우에만 존재

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

        internal FuncValue(M.ModuleName? moduleName, M.NamespacePath? namespacePath, TypeValue? outer, FuncDeclId? funcDeclId, M.FuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
        {
            this.moduleName = moduleName;
            this.namespacePath = namespacePath;
            this.outer = outer;
            this.funcDeclId = funcDeclId;
            this.funcInfo = funcInfo;
            this.typeArgs = typeArgs;
        }

        internal override int FillTypeEnv(TypeEnvBuilder builder)
        {
            int depth;
            if (outer != null)
                depth = outer.FillTypeEnv(builder) + 1;
            else
                depth = 0;

            for(int i = 0; i < typeArgs.Length; i++)
                builder.Add(depth, i, typeArgs[i]);

            return depth;
        }
        
        // class X<T> { void Func<U>(T t, U u, int x); }
        // X<int>.F<bool> => (int, bool, int)
        public ImmutableArray<TypeValue> GetParamTypes()
        {
            var typeEnv = MakeTypeEnv();

            var builder = ImmutableArray.CreateBuilder<TypeValue>(funcInfo.ParamTypes.Length);
            foreach (var paramType in funcInfo.ParamTypes)
            {   
                var paramTypeValue = typeValueFactory.MakeTypeValue(paramType);
                paramTypeValue.Apply(typeEnv);
                builder.Add(paramTypeValue);
            }

            return builder.MoveToImmutable();
        }

        public TypeValue GetRetType()
        {
            var typeEnv = MakeTypeEnv();
            var retTypeValue = typeValueFactory.MakeTypeValue(funcInfo.RetType);
            return retTypeValue.Apply(typeEnv);
        }

        // IR0 Func를 만들어 줍니다
        public R.Func MakeFunc()
        {
            // 1. GetFuncDeclId();
            // 2. TypeContext;            
            // TypeValue -> TypeContext

            if (moduleName != null && namespacePath != null)

            var declId = context.GetFuncDeclId()

        }   
    }
}
