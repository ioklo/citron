using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using Citron.Module;
using System.Linq;
using System.Diagnostics;

namespace Citron.Symbol
{
    public partial class EnumDeclSymbol : ITypeDeclSymbol
    {
        enum InitializeState
        {
            BeforeInitEnumElems,
            AfterInitEnumElems,
        }

        IDeclSymbolNode outer;
        Accessor accessor;

        Name name;
        ImmutableArray<Name> typeParams;

        ImmutableArray<EnumElemDeclSymbol> elems;
        ImmutableDictionary<Name, EnumElemDeclSymbol> elemsDict;
        
        InitializeState initState;

        public EnumDeclSymbol(IDeclSymbolNode outer, Accessor accessor, Name name, ImmutableArray<Name> typeParams)
        {
            this.outer = outer;
            this.accessor = accessor;
            this.name = name;
            this.typeParams = typeParams;

            this.initState = InitializeState.BeforeInitEnumElems;
        }
        
        public void InitElems(ImmutableArray<EnumElemDeclSymbol> elems)
        {
            Debug.Assert(initState == InitializeState.BeforeInitEnumElems);
            this.elems = elems;

            var builder = ImmutableDictionary.CreateBuilder<Name, EnumElemDeclSymbol>();
            foreach(var elem in elems)
                builder.Add(elem.GetName(), elem);

            this.elemsDict = builder.ToImmutable();

            initState = InitializeState.AfterInitEnumElems;
        }

        public EnumElemDeclSymbol? GetElem(Name memberName)
        {
            Debug.Assert(InitializeState.BeforeInitEnumElems < initState);
            return elemsDict.GetValueOrDefault(memberName);
        }

        public Name GetName()
        {
            return name;
        }

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public Name GetTypeParam(int i)
        {
            return typeParams[i];
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
            return outer;
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            Debug.Assert(InitializeState.BeforeInitEnumElems < initState);

            return typeParams.AsEnumerable().OfType<IDeclSymbolNode>()
                .Concat(elemsDict.Values);
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitEnum(this);
        }

        public Accessor GetAccessor()
        {
            return accessor;
        }

        public int GetElemCount()
        {
            Debug.Assert(InitializeState.BeforeInitEnumElems < initState);
            return elems.Length;
        }

        public EnumElemDeclSymbol GetElement(int index)
        {
            Debug.Assert(InitializeState.BeforeInitEnumElems < initState);
            return elems[index];
        }
    }
}