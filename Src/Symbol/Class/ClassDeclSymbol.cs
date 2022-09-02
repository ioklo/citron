using Citron.Collections;
using Citron.Module;
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

    [ImplementIEquatable]
    public partial class ClassDeclSymbol : ITypeDeclSymbol
    {   
        IHolder<IDeclSymbolNode> outerHolder;
        AccessModifier accessModifier;
        
        Name name;        
        ImmutableArray<TypeVarDeclSymbol> typeParams;
        IHolder<ImmutableArray<InterfaceSymbol>> interfacesHolder;

        ImmutableArray<ITypeDeclSymbol> memberTypesExceptTypeVars;

        ImmutableArray<ClassMemberFuncDeclSymbol> memberFuncs;
        IHolder<ImmutableArray<ClassConstructorDeclSymbol>> constructorsHolder; // 추가적으로 생성되는 것들 때문에 이렇게 했는데 약간 크게 잡은 경향이 있다
        ImmutableArray<ClassMemberVarDeclSymbol> memberVars;

        // Class선언 시점 typeEnv를 적용한 baseClass
        IHolder<ClassSymbol?> baseClassHolder;        
        IHolder<ClassConstructorDeclSymbol?> trivialConstructorHolder;

        [ExcludeComparison]
        TypeDict typeDict;

        [ExcludeComparison]
        FuncDict<ClassMemberFuncDeclSymbol> funcDict;
        
        public ClassDeclSymbol(
            IHolder<IDeclSymbolNode> outerHolder,
            AccessModifier accessModifier,
            Name name, ImmutableArray<TypeVarDeclSymbol> typeParams,
            IHolder<ClassSymbol?> baseClassHolder,
            IHolder<ImmutableArray<InterfaceSymbol>> interfacesHolder,
            ImmutableArray<ITypeDeclSymbol> memberTypesExceptTypeVars,
            ImmutableArray<ClassMemberFuncDeclSymbol> memberFuncs,
            ImmutableArray<ClassMemberVarDeclSymbol> memberVars,
            IHolder<ImmutableArray<ClassConstructorDeclSymbol>> constructorDeclsHolder,
            IHolder<ClassConstructorDeclSymbol?> trivialConstructorHolder)
        {
            this.outerHolder = outerHolder;
            this.accessModifier = accessModifier;
            this.name = name;
            this.typeParams = typeParams;
            this.baseClassHolder = baseClassHolder;
            this.interfacesHolder = interfacesHolder;
            this.memberTypesExceptTypeVars = memberTypesExceptTypeVars;
            this.memberFuncs = memberFuncs;
            this.memberVars = memberVars;

            this.constructorsHolder = constructorDeclsHolder;
            this.trivialConstructorHolder = trivialConstructorHolder;

            this.typeDict = TypeDict.Build(typeParams, memberTypesExceptTypeVars);
            this.funcDict = FuncDict.Build(memberFuncs);
        }

        public ClassConstructorDeclSymbol? GetDefaultConstructorDecl()
        {
            foreach (var decl in constructorsHolder.GetValue())
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
            return trivialConstructorHolder.GetValue();
        }        

        // Info자체에는 environment가 없으므로, typeEnv가 있어야
        public ClassSymbol? GetBaseClass()
        {            
            return baseClassHolder.GetValue();
        }        

        public ClassMemberFuncDeclSymbol? GetMemberFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return funcDict.Get(new DeclSymbolNodeName(name, typeParamCount, paramIds));
        }

        public ImmutableArray<ClassMemberFuncDeclSymbol> GetFuncs(Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        public ImmutableArray<ClassMemberFuncDeclSymbol> GetMemberFuncs()
        {
            return memberFuncs;
        }

        // include type parameters
        public ImmutableArray<ITypeDeclSymbol> GetMemberTypes()
        {
            var builder = ImmutableArray.CreateBuilder<ITypeDeclSymbol>(typeParams.Length + memberTypesExceptTypeVars.Length);
            builder.AddRange(typeParams.AsEnumerable());
            builder.AddRange(memberTypesExceptTypeVars.AsEnumerable());
            return builder.MoveToImmutable();
        }

        public int GetMemberVarCount()
        {
            return memberVars.Length;
        }

        public ClassMemberVarDeclSymbol GetMemberVar(int index)
        {
            return memberVars[index];
        }

        public Name GetName()
        {
            return name;
        }
        
        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitClass(this);
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outerHolder.GetValue();
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return typeDict.GetEnumerable().OfType<IDeclSymbolNode>()
                .Concat(memberFuncs.AsEnumerable())
                .Concat(memberVars.AsEnumerable());
        }
       
        public void Apply(IDeclSymbolNodeVisitor visitor)
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

        public AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }

        public int GetConstructorCount()
        {
            return constructorsHolder.GetValue().Length;
        }

        public ClassConstructorDeclSymbol GetConstructor(int index)
        {
            return constructorsHolder.GetValue()[index];
        }
    }
}
