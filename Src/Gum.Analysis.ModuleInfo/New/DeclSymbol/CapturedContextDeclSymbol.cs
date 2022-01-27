using Gum.Collections;
using M = Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Analysis
{
    //public class CapturedContextDeclSymbol : ITypeDeclSymbol
    //{
    //    IDeclSymbolNode outer;
    //    M.Name name;
    //    ImmutableArray<CapturedVarDeclSymbol> capturedVars;

    //    public CapturedContextDeclSymbol(IDeclSymbolNode outer, M.Name name, ImmutableArray<CapturedVarDeclSymbol> capturedVars)
    //    {
    //        this.outer = outer;
    //        this.name = name;
    //        this.capturedVars = capturedVars;
    //    }

    //    public void Apply(IDeclSymbolNodeVisitor visitor)
    //    {
    //        visitor.VisitCapturedContext(this);
    //    }

    //    public void Apply(ITypeDeclSymbolVisitor visitor)
    //    {
    //        visitor.VisitCapturedContext(this);
    //    }

    //    public M.AccessModifier GetAccessModifier()
    //    {
    //        return M.AccessModifier.Public;
    //    }

    //    public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
    //    {
    //        if (typeParamCount != 0 || !paramIds.IsEmpty)
    //            return null;

    //        foreach (var capturedVar in capturedVars)
    //            if (name.Equals(capturedVar.GetName()))
    //                return capturedVar;

    //        return null;
    //    }

    //    public DeclSymbolNodeName GetNodeName()
    //    {
    //        return new DeclSymbolNodeName(name, 0, default);
    //    }

    //    public IDeclSymbolNode? GetOuterDeclNode()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public CapturedVarDeclSymbol? GetCapturedVar(M.Name name)
    //    {
    //        foreach(var capturedVar in capturedVars)
    //        {
    //            if (name.Equals(capturedVar.GetName()))
    //                return capturedVar;
    //        }

    //        return null;
    //    }
    //}
}
