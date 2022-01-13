using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Collections;
using Pretune;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial class InterfaceDeclSymbol : ITypeDeclSymbolNode
    {
        IDeclSymbolNode outer;
        M.Name name;
        ImmutableArray<string> typeParams;

        public void Apply(ITypeDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitInterfaceDecl(this);
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return null;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public M.Name GetName()
        {
            return name;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitInterface(this);
        }
    }
}
