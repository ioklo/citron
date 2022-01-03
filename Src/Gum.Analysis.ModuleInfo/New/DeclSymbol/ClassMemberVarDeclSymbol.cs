using Pretune;
using System;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class ClassMemberVarDeclSymbol : IDeclSymbolNode
    {
        Lazy<ClassDeclSymbol> outer;
        M.AccessModifier accessModifier;
        bool bStatic;
        ITypeSymbolNode declType;
        M.Name name;

        // public static int s;
        public ClassMemberVarDeclSymbol(Lazy<ClassDeclSymbol> outer, M.AccessModifier accessModifier, bool bStatic, ITypeSymbolNode declType, M.Name name)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.bStatic = bStatic;
            this.declType = declType;
            this.name = name;
        }

        public ITypeSymbolNode GetDeclType()
        {
            return declType;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.Value;
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
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
    }
}
