using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Infra;
using Gum.Collections;
using Gum.Analysis;

namespace Gum.Analysis
{
    // X<int>.Y<short>.F_T_int_int<S>

    // F<>
    public class GlobalFuncSymbol : ISymbolNode
    {
        SymbolFactory factory;

        ITopLevelSymbolNode outer;

        // F_int_int
        GlobalFuncDeclSymbol decl;
        ImmutableArray<ITypeSymbolNode> typeArgs;

        TypeEnv typeEnv;        
        
        internal GlobalFuncSymbol(SymbolFactory factory, ITopLevelSymbolNode outer, GlobalFuncDeclSymbol funcDecl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = funcDecl;
            this.typeArgs = typeArgs;

            var outerTypeEnv = outer.GetTypeEnv();
            this.typeEnv = outerTypeEnv.AddTypeArgs(typeArgs);
        }
        public bool IsInstanceFunc()
        {
            return false;
        }

        public FuncParameter GetParameter(int index)
        {
            var parameter = decl.GetParameter(index);
            return parameter.Apply(typeEnv);
        }

        public FuncReturn GetReturn()
        {
            var @return = decl.GetReturn();
            return @return.Apply(typeEnv);
        }

        public GlobalFuncSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeGlobalFunc(appliedOuter, decl, appliedTypeArgs);
        }

        public R.Path.Nested MakeRPath()
        {
            var outerPath = outer.MakeRPath();
            var rtypeArgs = ImmutableArray.CreateRange<ITypeSymbolNode, R.Path>(typeArgs, typeArg => typeArg.MakeRPath());

            return decl.MakeRPath(outerPath, rtypeArgs);
        }

        public int GetParameterCount()
        {
            return decl.GetParameterCount();
        }

        public bool CheckAccess() // user는 지금 compilation되고 있으므로 internal이다
        {
            var accessModifier = decl.GetAccessModifier();

            switch (accessModifier)
            {
                case M.AccessModifier.Public: return true;
                case M.AccessModifier.Protected: throw new UnreachableCodeException(); // global에 이런게 나올수 없다
                case M.AccessModifier.Private:
                    return decl.IsInternal();

                default: throw new UnreachableCodeException();
            }
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        R.Path.Normal ISymbolNode.MakeRPath() => MakeRPath();

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            return typeArgs;
        }
    }
}
