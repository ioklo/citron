using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;

using M = Gum.CompileTime;

namespace Gum.Analysis
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

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return null;
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
    }
}
