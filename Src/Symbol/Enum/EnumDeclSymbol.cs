using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using Citron.Module;

namespace Citron.Symbol
{
    public partial class EnumDeclSymbol : ITypeDeclSymbol
    {
        IHolder<ITypeDeclSymbolContainer> containerHolder;

        Name name;
        ImmutableArray<string> typeParams;
        ImmutableArray<EnumElemDeclSymbol> elems;
        ImmutableDictionary<Name, EnumElemDeclSymbol> elemDict;

        public EnumDeclSymbol(IHolder<ITypeDeclSymbolContainer> containerHolder, Name name, ImmutableArray<string> typeParams, ImmutableArray<EnumElemDeclSymbol> elemDecls)
        {
            this.containerHolder = containerHolder;
            this.name = name;
            this.typeParams = typeParams;
            this.elems = elemDecls;

            var builder = ImmutableDictionary.CreateBuilder<Name, EnumElemDeclSymbol>();
            foreach(var elemDecl in elemDecls)
                builder.Add(elemDecl.GetName(), elemDecl);

            elemDict = builder.ToImmutable();
        }

        public EnumElemDeclSymbol? GetElem(Name memberName)
        {
            return elemDict.GetValueOrDefault(memberName);
        }

        public Name GetName()
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

        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitEnum(this);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return containerHolder.GetValue().GetOuterDeclNode();
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return elemDict.Values;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitEnum(this);
        }

        public AccessModifier GetAccessModifier()
        {
            return containerHolder.GetValue().GetAccessModifier();
        }

        public int GetElemCount()
        {
            return elems.Length;
        }

        public EnumElemDeclSymbol GetElement(int index)
        {
            return elems[index];
        }
    }
}