using Citron.Collections;
using System;

using Citron.Infra;
using System.Collections.Generic;
using System.Linq;
using Citron.Module;

namespace Citron.Symbol
{
    public record ClassConstructorDeclSymbol : IFuncDeclSymbol
    {
        ClassDeclSymbol outer;
        Accessor accessModifier;
        ImmutableArray<FuncParameter> parameters;
        bool bTrivial;

        public ClassConstructorDeclSymbol(ClassDeclSymbol outer, Accessor accessModifier, ImmutableArray<FuncParameter> parameters, bool bTrivial)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.parameters = parameters;
            this.bTrivial = bTrivial;
        }

        public Accessor GetAccessor()
        {
            return accessModifier;
        }

        public int GetParameterCount()
        {
            return parameters.Length;
        }

        public FuncParameter GetParameter(int index)
        {
            return parameters[index];
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(Name.Constructor, 0, parameters.MakeFuncParamIds());
        }
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClassConstructor(this);
        }

        public bool IsTrivial()
        {
            return bTrivial;
        }
    }
}
