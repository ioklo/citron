using Gum.Collections;
using Gum.IR0;
using System;
using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.Analysis
{
    public class StructMemberFuncSymbol : ISymbolNode
    {
        SymbolFactory factory;
        StructSymbol outer;
        StructMemberFuncDeclSymbol decl;
        ImmutableArray<ITypeSymbolNode> typeArgs;
        TypeEnv typeEnv;

        internal StructMemberFuncSymbol(SymbolFactory factory, StructSymbol outer, StructMemberFuncDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
            this.typeArgs = typeArgs;

            var outerTypeEnv = outer.GetTypeEnv();
            this.typeEnv = outerTypeEnv.AddTypeArgs(typeArgs);
        }

        public ISymbolNode Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeStructMemberFunc(appliedOuter, decl, appliedTypeArgs);
        }

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public R.Path.Nested MakeRPath()
        {
            var outerPath = outer.MakeRPath();
            var rtypeArgs = ImmutableArray.CreateRange<ITypeSymbolNode, R.Path>(typeArgs, typeArg => typeArg.MakeRPath());

            return decl.MakeRPath(outerPath, rtypeArgs);
        }

        public int GetTotalTypeParamCount()
        {
            return decl.GetTotalTypeParamCount();
        }

        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            throw new NotImplementedException();
        }

        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        public M.NormalTypeId MakeChildTypeId(M.Name name, ImmutableArray<M.TypeId> typeArgs)
        {
            throw new InvalidOperationException();
        }

        Path.Normal ISymbolNode.MakeRPath() => MakeRPath();        
    }
}