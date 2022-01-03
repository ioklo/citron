using Gum.Analysis;
using Gum.Collections;
using Gum.Infra;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public partial class EnumDeclSymbol : ITypeDeclSymbolNode
    {
        IHolder<IDeclSymbolNode> outer;

        M.Name name;
        ImmutableArray<string> typeParams;
        ImmutableDictionary<M.Name, EnumElemDeclSymbol> elemDict;

        public EnumDeclSymbol(IHolder<IDeclSymbolNode> outer, M.Name name, ImmutableArray<string> typeParams, ImmutableArray<EnumElemDeclSymbol> elemDecls)
        {
            this.outer = outer;
            this.name = name;
            this.typeParams = typeParams;

            var builder = ImmutableDictionary.CreateBuilder<M.Name, EnumElemDeclSymbol>();
            foreach(var elemDecl in elemDecls)
                builder.Add(elemDecl.GetName(), elemDecl);

            elemDict = builder.ToImmutable();
        }

        public EnumElemDeclSymbol? GetElem(M.Name memberName)
        {
            return elemDict.GetValueOrDefault(memberName);
        }

        public M.Name GetName()
        {
            return name;
        }

        public ImmutableArray<string> GetTypeParams()
        {
            return typeParams;
        }

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }
        
        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, default);
        }

        public void Apply(ITypeDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitEnumDecl(this);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            if (paramTypes.IsEmpty && typeParamCount == 0)
            {
                var elem = elemDict.GetValueOrDefault(name);
                if (elem != null)
                    return elem;
            }

            return null;
        }
    }
}