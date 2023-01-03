using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.Collections;
using Citron.Infra;
using Pretune;

using Citron.Module;

namespace Citron.Symbol
{
    [AutoConstructor]
    public partial class InterfaceDeclSymbol : ITypeDeclSymbol
    {
        IDeclSymbolNode outer;
        Accessor accessModifier;

        Name name;
        ImmutableArray<Name> typeParams;

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public Name GetTypeParam(int i)
        {
            return typeParams[i];
        }

        int IDeclSymbolNode.GetTypeParamCount()
            => GetTypeParamCount();

        Name IDeclSymbolNode.GetTypeParam(int i)
            => GetTypeParam(i);

        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitInterface(this);
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            // TODO: 아직 없는 것이다
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public Name GetName()
        {
            return name;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitInterface(this);
        }

        public Accessor GetAccessor()
        {
            return accessModifier;
        }
    }
}
