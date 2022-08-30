using Citron.Infra;
using Citron.Module;

namespace Citron.Symbol
{
    public record TypeDeclContainer<TOuterDeclSymbol>
        : ITypeDeclSymbolContainer, TypeDict.IHaveNodeName
        where TOuterDeclSymbol : IDeclSymbolNode
    {
        IHolder<TOuterDeclSymbol> outerHolder;
        AccessModifier accessModifier;
        ITypeDeclSymbol typeDecl;

        internal TypeDeclContainer(IHolder<TOuterDeclSymbol> outerHolder, AccessModifier accessModifier, ITypeDeclSymbol typeDecl)
        {
            this.outerHolder = outerHolder;
            this.accessModifier = accessModifier;
            this.typeDecl = typeDecl;
        }

        IDeclSymbolNode ITypeDeclSymbolContainer.GetOuterDeclNode()
            => GetOuter();

        // ITypeDeclSymbolContainer
        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            typeDecl.Apply(visitor);
        }

        // ITypeDeclSymbolContainer
        public AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return typeDecl.GetNodeName();
        }

        public TOuterDeclSymbol GetOuter()
        {
            return outerHolder.GetValue();
        }

        public IDeclSymbolNode GetNode()
        {
            return typeDecl;
        }
    }

    // 일단 -는 허용, 확장할 일이 있으면 감싸서 만든다
    //[AutoConstructor]
    //public partial class ClassMemberTypeDeclSymbol : ITypeDeclSymbolContainer, TypeDict.IHaveNodeName
    //{
    //    IHolder<ClassDeclSymbol> outerHolder;
    //    AccessModifier accessModifier;
    //    ITypeDeclSymbol typeDecl;

    //    IDeclSymbolNode ITypeDeclSymbolContainer.GetOuterDeclNode() => GetOuter();
        
    //    public ClassDeclSymbol GetOuter()
    //    {
    //        return outerHolder.GetValue();
    //    }

    //    public DeclSymbolNodeName GetNodeName() 
    //    {
    //        return typeDecl.GetNodeName();
    //    }

    //    public int GetTypeParamCount()
    //    {
    //        return typeDecl.GetTypeParamCount();
    //    }

    //    public IDeclSymbolNode GetNode()
    //    {
    //        return typeDecl;
    //    }

    //    public void Apply(ITypeDeclSymbolVisitor visitor)
    //    {
    //        typeDecl.Apply(visitor);
    //    }

    //    public AccessModifier GetAccessModifier()
    //    {
    //        return accessModifier;
    //    }
    //}
}
