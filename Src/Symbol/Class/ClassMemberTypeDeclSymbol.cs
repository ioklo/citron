using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Diagnostics;
using Citron.Module;

namespace Citron.Symbol
{
    public record ClassMemberTypeDeclSymbol(IHolder<ClassDeclSymbol> outerHolder, AccessModifier accessModifier, ITypeDeclSymbol typeDecl) 
        : TypeDeclContainer<ClassDeclSymbol>(outerHolder, accessModifier, typeDecl);
    
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
