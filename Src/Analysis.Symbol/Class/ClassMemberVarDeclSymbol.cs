using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using M = Citron.CompileTime;

namespace Citron.Analysis
{
    public class ClassMemberVarDeclSymbol : IDeclSymbolNode
    {
        IHolder<ClassDeclSymbol> outer;
        M.AccessModifier accessModifier;
        bool bStatic;
        IHolder<ITypeSymbol> declType;
        M.Name name;

        // public static int s;
        public ClassMemberVarDeclSymbol(IHolder<ClassDeclSymbol> outer, M.AccessModifier accessModifier, bool bStatic, IHolder<ITypeSymbol> declType, M.Name name)
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

        public M.Name GetName()
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

        public M.AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }
    }
}
