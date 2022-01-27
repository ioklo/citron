using Gum.Collections;
using Gum.Infra;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    //public class CapturedVarDeclSymbol : IDeclSymbolNode
    //{
    //    IHolder<CapturedContextDeclSymbol> outerHolder;
    //    ITypeSymbol declType;
    //    M.Name name;        

    //    public CapturedVarDeclSymbol(IHolder<CapturedContextDeclSymbol> outerHolder, ITypeSymbol declType, M.Name name)
    //    {
    //        this.outerHolder = outerHolder;
    //        this.declType = declType;
    //        this.name = name;
    //    }

    //    public void Apply(IDeclSymbolNodeVisitor visitor)
    //    {
    //        visitor.VisitCapturedVar(this);
    //    }

    //    public M.AccessModifier GetAccessModifier()
    //    {
    //        return M.AccessModifier.Private;
    //    }

    //    public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
    //    {
    //        return null;
    //    }

    //    public DeclSymbolNodeName GetNodeName()
    //    {
    //        return new DeclSymbolNodeName(name, 0, default);
    //    }

    //    public IDeclSymbolNode? GetOuterDeclNode()
    //    {
    //        return outerHolder.GetValue();
    //    }

    //    public ITypeSymbol GetDeclType()
    //    {
    //        return declType;
    //    }

    //    public M.Name GetName()
    //    {
    //        return name;
    //    }
    //}
}