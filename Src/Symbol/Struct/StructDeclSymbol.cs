using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Citron.Collections;
using Citron.Module;
using Citron.Infra;

using Pretune;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class StructDeclSymbol : ITypeDeclSymbol, ITypeDeclContainable
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

        StructSymbol? baseStruct;
        ImmutableArray<InterfaceSymbol> interfaces;

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

        public void InitBaseTypes(StructSymbol? baseStruct, ImmutableArray<InterfaceSymbol> interfaces)
        {
            Debug.Assert(initState == InitializeState.BeforeInitBaseTypes);
            this.baseStruct = baseStruct;
            this.interfaces = interfaces;
            this.initState = InitializeState.AfterInitBaseTypes;
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

        public StructSymbol? GetBaseStruct()
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

        public void Apply(ITypeDeclSymbolVisitor visitor)
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

        public void Apply(IDeclSymbolNodeVisitor visitor)
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
    }
}