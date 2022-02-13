using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Collections;

using M = Gum.CompileTime;

namespace Citron.Analysis
{
    public class InterfaceSymbol : ITypeSymbol
    {
        SymbolFactory factory;
        ISymbolNode outer;
        InterfaceDeclSymbol decl;
        ImmutableArray<ITypeSymbol> typeArgs;
        TypeEnv typeEnv;        

        internal InterfaceSymbol(SymbolFactory factory, ISymbolNode outer, InterfaceDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
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

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitInterface(this);
        }

        public ITypeDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return typeArgs;
        }

        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        public SymbolQueryResult QueryMember(M.Name name, int typeParamCount)
        {
            return SymbolQueryResult.NotFound.Instance;
        }

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => decl;
    }
}
