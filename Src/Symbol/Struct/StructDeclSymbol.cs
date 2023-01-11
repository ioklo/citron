using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Citron.Collections;
using Citron.Infra;

using Pretune;

namespace Citron.Symbol
{   
    public class StructDeclSymbol : ITypeDeclSymbol, ITypeDeclContainable, ICyclicEqualityComparableClass<StructDeclSymbol>
    {
        enum InitializeState
        {   
            BeforeInitBaseTypes,            
            AfterInitBaseTypes
        }

        IDeclSymbolNode outer;
        Accessor accessor;

        Name name;
        ImmutableArray<Name> typeParams;

        StructType? baseStruct;
        ImmutableArray<InterfaceType> interfaces;

        List<StructMemberVarDeclSymbol> memberVars;
        List<StructConstructorDeclSymbol> constructors;
        StructConstructorDeclSymbol? trivialConstructor;

        TypeDeclSymbolComponent typeComp;
        FuncDeclSymbolComponent<StructMemberFuncDeclSymbol> funcComp;

        InitializeState initState;

        public StructDeclSymbol(IDeclSymbolNode outer, Accessor accessor, Name name, ImmutableArray<Name> typeParams)
        {
            this.outer = outer;
            this.accessor = accessor;
            this.name = name;
            this.typeParams = typeParams;

            this.memberVars = new List<StructMemberVarDeclSymbol>();
            this.constructors = new List<StructConstructorDeclSymbol>();

            this.typeComp = TypeDeclSymbolComponent.Make();
            this.funcComp = FuncDeclSymbolComponent.Make<StructMemberFuncDeclSymbol>();

            this.initState = InitializeState.BeforeInitBaseTypes;
        }

        public void InitBaseTypes(StructType? baseStruct, ImmutableArray<InterfaceType> interfaces)
        {
            Debug.Assert(initState == InitializeState.BeforeInitBaseTypes);
            this.baseStruct = baseStruct;
            this.interfaces = interfaces;
            this.initState = InitializeState.AfterInitBaseTypes;
        }

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return typeParams.Length;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            return typeParams[i];
        }

        public void AddMemberVar(StructMemberVarDeclSymbol memberVar)
        {
            Debug.Assert(this == memberVar.GetOuterDeclNode());
            memberVars.Add(memberVar);
        }
        
        public void AddConstructor(StructConstructorDeclSymbol constructor)
        {
            Debug.Assert(this == constructor.GetOuterDeclNode());

            constructors.Add(constructor);

            if (constructor.IsTrivial())
            {
                Debug.Assert(trivialConstructor == null);
                this.trivialConstructor = constructor;
            }
        }

        // include type parameters
        public IEnumerable<ITypeDeclSymbol> GetMemberTypes()
        {
            return typeComp.GetEnumerable();
        }

        public Name GetName()
        {
            return name;
        }

        public IEnumerable<StructMemberFuncDeclSymbol> GetFuncs()
        {
            return funcComp.GetEnumerable();
        }

        public StructConstructorDeclSymbol? GetTrivialConstructor()
        {
            return trivialConstructor;
        }

        public StructType? GetBaseStruct()
        {
            return baseStruct;
        }
        
        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, default);
        }

        public int GetMemberVarCount()
        {
            return memberVars.Count;
        }

        public StructMemberVarDeclSymbol GetMemberVar(int index)
        {
            return memberVars[index];
        }

        public void Accept(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitStruct(this);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer;
        }
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return typeComp.GetEnumerable().OfType<IDeclSymbolNode>()
                .Concat(funcComp.GetEnumerable())
                .Concat(memberVars);
        }

        public void Accept(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStruct(this);
        }

        public int GetConstructorCount()
        {
            return constructors.Count;
        }

        public StructConstructorDeclSymbol GetConstructor(int index)
        {
            return constructors[index];
        }

        public Accessor GetAccessor()
        {
            return accessor;
        }

        public ITypeDeclSymbol? GetType(Name name, int typeParamCount)
            => typeComp.GetType(name, typeParamCount);

        public void AddType(ITypeDeclSymbol typeDecl)
            => typeComp.AddType(typeDecl);

        public StructMemberFuncDeclSymbol? GetFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
            => funcComp.GetFunc(name, typeParamCount, paramIds);

        public IEnumerable<StructMemberFuncDeclSymbol> GetFuncs(Name name, int minTypeParamCount)
            => funcComp.GetFuncs(name, minTypeParamCount);

        public void AddFunc(StructMemberFuncDeclSymbol memberFunc)
            => funcComp.AddFunc(memberFunc);

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is StructDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<ITypeDeclSymbol>.CyclicEquals(ITypeDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is StructDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<StructDeclSymbol>.CyclicEquals(StructDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);        

        bool CyclicEquals(StructDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessor.Equals(other.accessor))
                return false;

            if (!name.Equals(other.name))
                return false;

            if (!typeParams.Equals(other.typeParams))
                return false;

            if (!context.CompareClass(baseStruct, other.baseStruct))
                return false;

            if (!interfaces.CyclicEqualsClassItem(ref other.interfaces, ref context))
                return false;

            if (!memberVars.CyclicEqualsClassItem(other.memberVars, ref context))
                return false;

            if (!constructors.CyclicEqualsClassItem(other.constructors, ref context))
                return false;

            if (!context.CompareClass(trivialConstructor, other.trivialConstructor))
                return false;

            if (!typeComp.CyclicEquals(ref other.typeComp, ref context))
                return false;

            if (!funcComp.CyclicEquals(ref other.funcComp, ref context))
                return false;

            if (!initState.Equals(other.initState))
                return false;

            return true;
        }


    }
}