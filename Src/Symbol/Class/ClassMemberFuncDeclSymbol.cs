using System;
using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Citron.Symbol
{   
    public record ClassMemberFuncDeclSymbol 
        : IFuncDeclSymbol
        , ICyclicEqualityComparableClass<ClassMemberFuncDeclSymbol>
    {
        enum InitializeState
        {
            BeforeInitFuncReturnAndParams,
            AfterInitFuncReturnAndParams,
        }

        ClassDeclSymbol outer;
        Accessor accessModifier;
        FuncReturn funcReturn;
        Name name;
        ImmutableArray<Name> typeParams;
        ImmutableArray<FuncParameter> parameters;        
        bool bStatic;

        InitializeState initState;

        public ClassMemberFuncDeclSymbol(
            ClassDeclSymbol outer, 
            Accessor accessModifier, 
            Name name,
            ImmutableArray<Name> typeParams,
            bool bStatic)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.name = name;
            this.typeParams = typeParams;
            this.bStatic = bStatic;

            this.initState = InitializeState.BeforeInitFuncReturnAndParams;
        }

        public void InitFuncReturnAndParams(FuncReturn funcReturn, ImmutableArray<FuncParameter> parameters)
        {
            Debug.Assert(initState == InitializeState.BeforeInitFuncReturnAndParams);

            this.funcReturn = funcReturn;
            this.parameters = parameters;

            initState = InitializeState.AfterInitFuncReturnAndParams;
        }
        
        public Accessor GetAccessor()
        {
            return accessModifier;
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

        public FuncReturn GetReturn()
        {
            Debug.Assert(InitializeState.BeforeInitFuncReturnAndParams < initState);
            return funcReturn;
        }

        public int GetTypeParamCount()
        {   
            return typeParams.Length;
        }

        public Name GetTypeParam(int i)
        {
            return typeParams[i];
        }

        public bool IsStatic()
        {
            return bStatic;
        }

        public DeclSymbolNodeName GetNodeName()
        {   
            return new DeclSymbolNodeName(name, typeParams.Length, parameters.MakeFuncParamIds());
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        public void Accept(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClassMemberFunc(this);
        }

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ClassMemberFuncDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncDeclSymbol>.CyclicEquals(IFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is ClassMemberFuncDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<ClassMemberFuncDeclSymbol>.CyclicEquals(ClassMemberFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(ClassMemberFuncDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessModifier.Equals(other.accessModifier))
                return false;

            if (!funcReturn.CyclicEquals(ref other.funcReturn, ref context))
                return false;

            if (!name.Equals(other.name))
                return false;

            if (!typeParams.Equals(other.typeParams))
                return false;

            if (!parameters.CyclicEqualsStructItem(ref other.parameters, ref context))
                return false;

            if (bStatic.Equals(other.bStatic))
                return false;

            if (initState.Equals(other.initState))
                return false;

            return true;
        }
    }
}
