using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Collections;

using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.Analysis
{
    public class InterfaceSymbol : ITypeSymbolNode
    {
        SymbolFactory factory;
        ISymbolNode outer;
        InterfaceDeclSymbol decl;
        ImmutableArray<ITypeSymbolNode> typeArgs;
        TypeEnv typeEnv;        

        internal InterfaceSymbol(SymbolFactory factory, ISymbolNode outer, InterfaceDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
            this.typeArgs = typeArgs;
            this.typeEnv = outer.GetTypeEnv().AddTypeArgs(typeArgs);
        }

        public InterfaceSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeInterface(appliedOuter, decl, appliedTypeArgs);
        }

        public ITypeDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            return typeArgs;
        }

        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        public R.Path.Nested MakeRPath()
        {
            var rname = RItemFactory.MakeName(decl.GetName());
            var rtypeArgs = RItemFactory.MakeRTypes(typeArgs);

            return new R.Path.Nested(outer.MakeRPath(), rname, new R.ParamHash(decl.GetTypeParamCount(), default), rtypeArgs);
        }

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeSymbolNode ITypeSymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => decl;
        R.Path.Normal ISymbolNode.MakeRPath() => MakeRPath();        
    }
}
