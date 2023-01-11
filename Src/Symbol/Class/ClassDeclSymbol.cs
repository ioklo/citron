using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Symbol
{
    // StructDel? outer = null;
    // var innerClass = new ClassDeclSymbol(() => outer!, ....);
    // outer = new StructDecl(innerClass);
    
    public class ClassDeclSymbol 
        : ITypeDeclSymbol
        , ITypeDeclContainable
        , ICyclicEqualityComparableClass<ClassDeclSymbol>
    {   
        enum InitializeState
        {
            BeforeInitBaseTypes,
            AfterInitBaseTypes,
        }

        IDeclSymbolNode outer;
        Accessor accessModifier;
        
        Name name;        
        ImmutableArray<Name> typeParams;
        
        ClassType? baseClass; // Class선언 시점 typeEnv를 적용한 baseClass
        ImmutableArray<InterfaceType> interfaces;
        
        List<ClassMemberVarDeclSymbol> memberVars;

        List<ClassConstructorDeclSymbol> constructors;
        ClassConstructorDeclSymbol? trivialConstructor;
        
        TypeDeclSymbolComponent typeComp;
        FuncDeclSymbolComponent<ClassMemberFuncDeclSymbol> funcComp;
        InitializeState initState;

        public ClassDeclSymbol(IDeclSymbolNode outer, Accessor accessModifier, Name name, ImmutableArray<Name> typeParams)
        {
            this.outer = outer;
            this.accessModifier = accessModifier;
            this.name = name;

            this.memberVars = new List<ClassMemberVarDeclSymbol>();
            this.constructors = new List<ClassConstructorDeclSymbol>();

            this.typeParams = typeParams;

            this.typeComp = TypeDeclSymbolComponent.Make();
            this.funcComp = FuncDeclSymbolComponent.Make<ClassMemberFuncDeclSymbol>();

            this.initState = InitializeState.BeforeInitBaseTypes;
        }

        public void InitBaseTypes(ClassType? baseClass, ImmutableArray<InterfaceType> interfaces)
        {
            Debug.Assert(initState == InitializeState.BeforeInitBaseTypes);

            this.baseClass = baseClass;
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

        public ClassConstructorDeclSymbol? GetDefaultConstructorDecl()
        {
            foreach (var decl in constructors)
            {
                if (decl.GetParameterCount() == 0)
                    return decl;
            }

            return null;
        }

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public ClassConstructorDeclSymbol? GetTrivialConstructor()
        {
            return trivialConstructor;
        }        

        // Info자체에는 environment가 없으므로, typeEnv가 있어야
        public ClassType? GetBaseClass()
        {
            Debug.Assert(InitializeState.BeforeInitBaseTypes < initState);
            return baseClass;
        }

        // include type parameters
        public ImmutableArray<ITypeDeclSymbol> GetMemberTypes()
        {
            return typeComp.GetEnumerable().ToImmutableArray();
        }        

        public int GetMemberVarCount()
        {
            return memberVars.Count;
        }

        public ClassMemberVarDeclSymbol GetMemberVar(int index)
        {
            return memberVars[index];
        }

        public void AddMemberVar(ClassMemberVarDeclSymbol declSymbol)
        {
            Debug.Assert(this == declSymbol.GetOuterDeclNode());
            memberVars.Add(declSymbol);
        }

        public Name GetName()
        {
            return name;
        }
        
        public void Accept(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitClass(this);
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, default);
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
            visitor.VisitClass(this);
        }

        public ClassMemberVarDeclSymbol? GetMemberVar(Name name)
        {
            foreach(var memberVar in memberVars)
            {
                if (name.Equals(memberVar.GetName()))
                    return memberVar;
            }

            return null;
        }

        public Accessor GetAccessor()
        {
            return accessModifier;
        }

        public int GetConstructorCount()
        {
            return constructors.Count;
        }

        public ClassConstructorDeclSymbol GetConstructor(int index)
        {
            return constructors[index];
        }

        public void AddConstructor(ClassConstructorDeclSymbol constructor)
        {
            Debug.Assert(this == constructor.GetOuterDeclNode());

            constructors.Add(constructor);

            if (constructor.IsTrivial())
            {
                Debug.Assert(trivialConstructor == null);
                this.trivialConstructor = constructor;
            }
        }

        public void AddType(ITypeDeclSymbol typeDecl)
            => typeComp.AddType(typeDecl);

        public void AddFunc(ClassMemberFuncDeclSymbol declSymbol)
            => funcComp.AddFunc(declSymbol);

        public ClassMemberFuncDeclSymbol? GetFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
            => funcComp.GetFunc(name, typeParamCount, paramIds);

        public IEnumerable<ClassMemberFuncDeclSymbol> GetMemberFuncs()
            => funcComp.GetFuncs();

        public IEnumerable<ClassMemberFuncDeclSymbol> GetMemberFuncs(Name name, int minTypeParamCount)
            => funcComp.GetFuncs(name, minTypeParamCount);

        bool ICyclicEqualityComparableClass<ClassDeclSymbol>.CyclicEquals(ClassDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool ICyclicEqualityComparableClass<ITypeDeclSymbol>.CyclicEquals(ITypeDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is ClassDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ClassDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool CyclicEquals(ClassDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessModifier.Equals(other.accessModifier))
                return false;

            if (!name.Equals(other.name))
                return false;

            if (!typeParams.Equals(other.typeParams))
                return false;

            if (!context.CompareClass(baseClass, other.baseClass))
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
