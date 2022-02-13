using Citron.Collections;
using M = Citron.CompileTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Analysis
{
    // task / await 에서 쓰이는 
    //public class CapturedContextSymbol : ITypeSymbol
    //{
    //    SymbolFactory factory;
    //    ISymbolNode outer;
    //    CapturedContextDeclSymbol decl;

    //    ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    //    ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
    //    IDeclSymbolNode? ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();

    //    public CapturedContextSymbol(SymbolFactory factory, ISymbolNode outer, CapturedContextDeclSymbol decl)
    //    {
    //        this.factory = factory;
    //        this.outer = outer;
    //        this.decl = decl;
    //    }

    //    public CapturedContextSymbol Apply(TypeEnv typeEnv)
    //    {
    //        var appliedOuter = outer.Apply(typeEnv);
    //        return factory.MakeCapturedContext(appliedOuter, decl);
    //    }

    //    public void Apply(ITypeSymbolVisitor visitor)
    //    {
    //        visitor.VisitCapturedContext(this);
    //    }

    //    public ITypeDeclSymbol? GetDeclSymbolNode()
    //    {
    //        return decl;
    //    }

    //    public ISymbolNode? GetOuter()
    //    {
    //        return outer;
    //    }

    //    public ImmutableArray<ITypeSymbol> GetTypeArgs()
    //    {
    //        return default;
    //    }

    //    public TypeEnv GetTypeEnv()
    //    {
    //        return outer.GetTypeEnv();
    //    }

    //    public SymbolQueryResult QueryMember(M.Name memberName, int typeParamCount)
    //    {
    //        var capturedVarDecl = decl.GetCapturedVar(memberName);
    //        if (capturedVarDecl != null)
    //        {
    //            if (typeParamCount != 0)
    //                return SymbolQueryResult.Error.VarWithTypeArg.Instance;

    //            var memberVarSymbol = factory.MakeCapturedVar(this, capturedVarDecl);
    //            return new SymbolQueryResult.CapturedVar(memberVarSymbol);
    //        }

    //        return SymbolQueryResult.Error.NotFound.Instance;
    //    }
    //}
}
