using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citron.Infra;
using Citron.Module;
using Pretune;

namespace Citron.Symbol
{
    [AutoConstructor, ImplementIEquatable]
    public partial class TypeVarDeclSymbol : ITypeDeclSymbol, IDeclSymbolNode
    {
        IHolder<IDeclSymbolNode> outerHolder; // 타입일 수도, 함수일 수도
        Name name;

        IDeclSymbolNode? IDeclSymbolNode.GetOuterDeclNode()
            => GetOuterDeclNode();

        void ITypeDeclSymbol.Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitTypeVar(this);
        }

        void IDeclSymbolNode.Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitTypeVar(this);
        }

        AccessModifier IDeclSymbolNode.GetAccessModifier()
        {
            return AccessModifier.Public;
        }

        IEnumerable<IDeclSymbolNode> IDeclSymbolNode.GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        DeclSymbolNodeName IDeclSymbolNode.GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outerHolder.GetValue();
        }
    }
}
