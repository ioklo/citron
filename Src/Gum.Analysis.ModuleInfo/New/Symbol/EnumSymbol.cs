using System;
using Gum.Collections;

using M = Gum.CompileTime;
using Pretune;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    public partial class EnumSymbol : ITypeSymbol
    {
        SymbolFactory factory;

        ISymbolNode outer;
        EnumDeclSymbol decl;
        ImmutableArray<ITypeSymbol> typeArgs;

        TypeEnv typeEnv;

        internal EnumSymbol(SymbolFactory factory, ISymbolNode outer, EnumDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
            this.typeArgs = typeArgs;

            var outerTypeEnv = outer.GetTypeEnv();
            this.typeEnv = outerTypeEnv.AddTypeArgs(typeArgs);
        }

        public EnumSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeEnum(appliedOuter, decl, appliedTypeArgs);
        }

        //
        public SymbolQueryResult QueryMember(M.Name memberName, int typeParamCount) 
        {
            // shortcut
            if (typeParamCount != 0)
                return SymbolQueryResult.NotFound.Instance;

            var elemDecl = decl.GetElem(memberName);
            if (elemDecl == null) return SymbolQueryResult.NotFound.Instance;

            var elem = factory.MakeEnumElem(this, elemDecl);

            return new SymbolQueryResult.EnumElem(elem);
        }

        public EnumElemSymbol? GetElement(string name)
        {
            var elemDecl = decl.GetElem(new M.Name.Normal(name));
            if (elemDecl == null) return null;

            return factory.MakeEnumElem(this, elemDecl);
        }

        public ITypeSymbol? GetMemberType(M.Name memberName, ImmutableArray<ITypeSymbol> typeArgs) 
        {
            // shortcut
            if (typeArgs.Length != 0)
                return null;

            var elemDecl = decl.GetElem(memberName);
            if (elemDecl == null) return null;

            return factory.MakeEnumElem(this, elemDecl);
        }
        
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => decl;
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        public ITypeDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }        
        
        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }        
        
        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return typeArgs;
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitEnum(this);
        }
    }
}
