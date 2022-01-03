using Gum.Collections;
using System.Diagnostics;

namespace Gum.Analysis
{
    class MemberTypeInstantiator : ITypeDeclSymbolNodeVisitor
    {
        // input
        SymbolFactory factory;
        ISymbolNode outer;
        ImmutableArray<ITypeSymbolNode> typeArgs;

        // output
        ITypeSymbolNode? result;

        MemberTypeInstantiator(SymbolFactory factory, ISymbolNode outer, ImmutableArray<ITypeSymbolNode> typeArgs) 
        {
            this.factory = factory;
            this.outer = outer;
            this.typeArgs = typeArgs;
            this.result = null;
        }

        public static ITypeSymbolNode Instantiate(SymbolFactory factory, ISymbolNode outer, ClassMemberTypeDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            var instantiator = new MemberTypeInstantiator(factory, outer, typeArgs);
            decl.Apply(instantiator);
            Debug.Assert(instantiator.result != null);
            return instantiator.result;
        }

        public static ITypeSymbolNode Instantiate(SymbolFactory factory, ISymbolNode outer, StructMemberTypeDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            var instantiator = new MemberTypeInstantiator(factory, outer, typeArgs);
            decl.Apply(instantiator);
            Debug.Assert(instantiator.result != null);
            return instantiator.result;
        }

        public void VisitClassDecl(ClassDeclSymbol classDecl)
        {
            result = factory.MakeClass(outer, classDecl, typeArgs);
        }

        public void VisitEnumDecl(EnumDeclSymbol enumDecl)
        {
            result = factory.MakeEnum(outer, enumDecl, typeArgs);
        }

        public void VisitEnumElemDecl(EnumElemDeclSymbol enumElemDecl)
        {
            var outerEnum = outer as EnumSymbol;
            Debug.Assert(outerEnum != null);

            result = factory.MakeEnumElem(outerEnum, enumElemDecl);
        }

        public void VisitStructDecl(StructDeclSymbol structDecl)
        {
            result = factory.MakeStruct(outer, structDecl, typeArgs);
        }
    }

}
