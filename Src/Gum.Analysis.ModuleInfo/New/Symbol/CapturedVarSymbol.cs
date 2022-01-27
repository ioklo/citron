using Gum.Collections;
using Gum.CompileTime;

namespace Gum.Analysis
{
    //public class CapturedVarSymbol : ISymbolNode
    //{
    //    SymbolFactory factory;
    //    CapturedContextSymbol outer;
    //    CapturedVarDeclSymbol decl;

    //    ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

    //    public CapturedVarSymbol(SymbolFactory factory, CapturedContextSymbol outer, CapturedVarDeclSymbol decl)
    //    {
    //        this.factory = factory;
    //        this.outer = outer;
    //        this.decl = decl;
    //    }

    //    public CapturedVarSymbol Apply(TypeEnv typeEnv)
    //    {
    //        var appliedOuter = outer.Apply(typeEnv);
    //        return factory.MakeCapturedVar(appliedOuter, decl);
    //    }

    //    public void Apply(ITypeSymbolVisitor visitor)
    //    {
    //        visitor.VisitCapturedVar(this);
    //    }

    //    public IDeclSymbolNode? GetDeclSymbolNode()
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

    //    public SymbolQueryResult QueryMember(Name memberName, int typeParamCount)
    //    {
    //        return SymbolQueryResult.NotFound.Instance;
    //    }
        
    //    IDeclSymbolNode? ISymbolNode.GetDeclSymbolNode()
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}
}
