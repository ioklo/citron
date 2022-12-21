using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using Citron.Module;
using System.Diagnostics;

namespace Citron.Symbol
{
    public class StructMemberFuncDeclSymbol : IFuncDeclSymbol
    {
        enum InitializeState
        {
            BeforeInitFuncReturnAndParams,
            AfterInitFuncReturnAndParams,
        }

        StructDeclSymbol outer;

        Accessor accessModifier;
        bool bStatic;
        FuncReturn funcReturn;
        Name name;
        ImmutableArray<Name> typeParams;
        ImmutableArray<FuncParameter> parameters;

        InitializeState initState;

        public StructMemberFuncDeclSymbol(
            StructDeclSymbol outer, 
            Accessor accessModifier,             
            bool bStatic,            
            Name name,
            ImmutableArray<Name> typeParams)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;            
            this.bStatic = bStatic;            
            this.name = name;
            this.typeParams = typeParams;

            this.initState = InitializeState.BeforeInitFuncReturnAndParams;
        }

        public void InitFuncReturnAndParams(FuncReturn @return, ImmutableArray<FuncParameter> parameters)
        {
            Debug.Assert(initState == InitializeState.BeforeInitFuncReturnAndParams);

            this.funcReturn = @return;
            this.parameters = parameters;

            initState = InitializeState.AfterInitFuncReturnAndParams;
        }

        public int GetParameterCount()
        {
            Debug.Assert(InitializeState.BeforeInitFuncReturnAndParams < initState);
            return parameters.Length;
        }

        public FuncParameter GetParameter(int index)
        {
            Debug.Assert(InitializeState.BeforeInitFuncReturnAndParams < initState);
            return parameters[index];
        }

        public DeclSymbolNodeName GetNodeName()
        {
            Debug.Assert(InitializeState.BeforeInitFuncReturnAndParams < initState);
            return new DeclSymbolNodeName(name, typeParams.Length, parameters.MakeFuncParamIds());
        }
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }
        
        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public bool IsStatic()
        {
            return bStatic;
        }        

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructMemberFunc(this);
        }

        public FuncReturn GetReturn()
        {
            Debug.Assert(InitializeState.BeforeInitFuncReturnAndParams < initState);
            return funcReturn;
        }

        public Accessor GetAccessor()
        {
            return accessModifier;
        }
    }
}