using Citron.Collections;
using Pretune;
using System;
using Citron.Module;
using Citron.Infra;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class GlobalFuncDeclSymbol : IFuncDeclSymbol
    {
        enum InitializeState
        {
            BeforeInitFuncReturnAndParams,
            AfterInitFuncReturnAndParams,
        }

        // module or namespace
        ITopLevelDeclSymbolNode outer;

        Accessor accessModifier;
        FuncReturn @return;
        Name name;

        ImmutableArray<Name> typeParams;
        ImmutableArray<FuncParameter> parameters;

        InitializeState initState;

        public GlobalFuncDeclSymbol(
            ITopLevelDeclSymbolNode outer, Accessor accessModifier, 
            Name name,
            ImmutableArray<Name> typeParams)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.name = name;
            this.typeParams = typeParams; 

            this.initState = InitializeState.BeforeInitFuncReturnAndParams;
        }

        public void InitFuncReturnAndParams(FuncReturn @return, ImmutableArray<FuncParameter> parameters)
        {
            Debug.Assert(initState == InitializeState.BeforeInitFuncReturnAndParams);

            this.@return = @return;
            this.parameters = parameters;

            this.initState = InitializeState.AfterInitFuncReturnAndParams;
        }

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
        
        public Accessor GetAccessor()
        {
            return accessModifier;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            Debug.Assert(InitializeState.AfterInitFuncReturnAndParams < initState);
            return new DeclSymbolNodeName(name, typeParams.Length, parameters.MakeFuncParamIds());
        }
        
        public int GetParameterCount()
        {
            Debug.Assert(InitializeState.AfterInitFuncReturnAndParams < initState);
            return parameters.Length;
        }

        public FuncParameter GetParameter(int index)
        {
            Debug.Assert(InitializeState.AfterInitFuncReturnAndParams < initState);
            return parameters[index];
        }

        public FuncReturn GetReturn()
        {
            Debug.Assert(InitializeState.AfterInitFuncReturnAndParams < initState);
            return @return;
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return typeParams.AsEnumerable().OfType<IDeclSymbolNode>();
        }        

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitGlobalFunc(this);
        }
    }
}