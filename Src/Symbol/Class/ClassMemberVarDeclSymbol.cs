using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using Citron.Module;

namespace Citron.Symbol
{
    public class ClassMemberVarDeclSymbol : IDeclSymbolNode
    {
        IHolder<ClassDeclSymbol> outer;
        AccessModifier accessModifier;
        bool bStatic;
        IHolder<ITypeSymbol> declType;
        Name name;

        // public static int s;
        public ClassMemberVarDeclSymbol(IHolder<ClassDeclSymbol> outer, AccessModifier accessModifier, bool bStatic, IHolder<ITypeSymbol> declType, Name name)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.bStatic = bStatic;
            this.declType = declType;
            this.name = name;
        }

        public ITypeSymbol GetDeclType()
        {
            return declType.GetValue();
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }        

        public Name GetName()
        {
            return name;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClassMemberVar(this);
        }

        public bool IsStatic()
        {
            return bStatic;
        }

        public AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }
    }
}
