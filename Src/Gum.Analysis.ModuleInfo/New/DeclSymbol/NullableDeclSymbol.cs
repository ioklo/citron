using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;
using Gum.Collections;

namespace Gum.Analysis
{
    //public class NullableDeclSymbol : ITypeDeclSymbol
    //{
    //    public void Apply(ITypeDeclSymbolVisitor visitor)
    //    {
    //        visitor.VisitNullable(this);
    //    }

    //    public void Apply(IDeclSymbolNodeVisitor visitor)
    //    {
    //        visitor.VisitNullable(this);
    //    }

    //    public M.AccessModifier GetAccessModifier()
    //    {
    //        return M.AccessModifier.Public;
    //    }

    //    public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
    //    {
    //        return null;
    //    }

    //    public DeclSymbolNodeName GetNodeName()
    //    {
    //        return new DeclSymbolNodeName(M.Name.Nullable, 1, default);
    //    }

    //    public IDeclSymbolNode? GetOuterDeclNode()
    //    {
    //        return null;
    //    }
    //}
}
