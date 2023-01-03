using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.Collections;

using Citron.Module;

namespace Citron.Symbol
{
    public class InterfaceSymbol : ITypeSymbol
    {
        SymbolFactory factory;
        ISymbolNode outer;
        InterfaceDeclSymbol decl;
        ImmutableArray<IType> typeArgs;
        TypeEnv typeEnv;        

        internal InterfaceSymbol(SymbolFactory factory, ISymbolNode outer, InterfaceDeclSymbol decl, ImmutableArray<IType> typeArgs)
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

        public IType? GetMemberType(Name memberName, ImmutableArray<IType> typeArgs)
        {
            throw new NotImplementedException();
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public IType GetTypeArg(int index)
        {
            return typeArgs[index];
        }

        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        public IType MakeType()
        {
            return new InterfaceType(this);
        }

        public SymbolQueryResult QueryMember(Name name, int typeParamCount)
        {
            return SymbolQueryResults.NotFound;
        }

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => decl;
    }
}
