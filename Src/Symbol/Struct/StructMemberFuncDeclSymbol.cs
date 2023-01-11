using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Citron.Symbol
{
    public class StructMemberFuncDeclSymbol : IFuncDeclSymbol, ICyclicEqualityComparableClass<StructMemberFuncDeclSymbol>
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

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return typeParams.Length;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            return typeParams[i];
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

        public void Accept(IDeclSymbolNodeVisitor visitor)
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

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is StructMemberFuncDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncDeclSymbol>.CyclicEquals(IFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is StructMemberFuncDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<StructMemberFuncDeclSymbol>.CyclicEquals(StructMemberFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(StructMemberFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessModifier.Equals(other.accessModifier))
                return false;

            if (!bStatic.Equals(other.bStatic))
                return false;

            if (!funcReturn.CyclicEquals(ref other.funcReturn, ref context))
                return false;

            if (!name.Equals(other.name))
                return false;

            if (!typeParams.Equals(other.typeParams))
                return false;

            if (!parameters.CyclicEqualsStructItem(ref other.parameters, ref context))
                return false;

            if (!initState.Equals(other.initState))
                return false;

            return true;
        }
    }
}