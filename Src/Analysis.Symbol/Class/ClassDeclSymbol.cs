using Citron.Analysis;
using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Citron.CompileTime;

namespace Citron.Analysis
{
    // StructDel? outer = null;
    // var innerClass = new ClassDeclSymbol(() => outer!, ....);
    // outer = new StructDecl(innerClass);

    [ImplementIEquatable]
    public partial class ClassDeclSymbol : ITypeDeclSymbol
    {
        IHolder<ITypeDeclSymbolContainer> containerHolder;
        
        M.Name name;
        ImmutableArray<string> typeParams;
        IHolder<ImmutableArray<InterfaceSymbol>> interfacesHolder;

        ImmutableArray<ClassMemberTypeDeclSymbol> memberTypes;
        ImmutableArray<ClassMemberFuncDeclSymbol> memberFuncs;
        IHolder<ImmutableArray<ClassConstructorDeclSymbol>> constructorsHolder; // 추가적으로 생성되는 것들 때문에 이렇게 했는데 약간 크게 잡은 경향이 있다
        ImmutableArray<ClassMemberVarDeclSymbol> memberVars;

        // Class선언 시점 typeEnv를 적용한 baseClass
        IHolder<ClassSymbol?> baseClassHolder;        
        IHolder<ClassConstructorDeclSymbol?> trivialConstructorHolder;

        [ExcludeComparison]
        TypeDict<ClassMemberTypeDeclSymbol> typeDict;

        [ExcludeComparison]
        FuncDict<ClassMemberFuncDeclSymbol> funcDict;
        
        public ClassDeclSymbol(
            IHolder<ITypeDeclSymbolContainer> containerHolder,
            M.Name name, ImmutableArray<string> typeParams,
            IHolder<ClassSymbol?> baseClassHolder,
            IHolder<ImmutableArray<InterfaceSymbol>> interfacesHolder,
            ImmutableArray<ClassMemberTypeDeclSymbol> memberTypes,
            ImmutableArray<ClassMemberFuncDeclSymbol> memberFuncs,
            ImmutableArray<ClassMemberVarDeclSymbol> memberVars,
            IHolder<ImmutableArray<ClassConstructorDeclSymbol>> constructorDeclsHolder,
            IHolder<ClassConstructorDeclSymbol?> trivialConstructorHolder)
        {
            this.containerHolder = containerHolder;
            this.name = name;
            this.typeParams = typeParams;
            this.baseClassHolder = baseClassHolder;
            this.interfacesHolder = interfacesHolder;
            this.memberTypes = memberTypes;
            this.memberFuncs = memberFuncs;
            this.memberVars = memberVars;

            this.constructorsHolder = constructorDeclsHolder;
            this.trivialConstructorHolder = trivialConstructorHolder;

            this.typeDict = TypeDict.Build(memberTypes);
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

        public ClassMemberTypeDeclSymbol? GetMemberTypeDecl(M.Name name, int typeParamCount)
        {
            return typeDict.Get(new DeclSymbolNodeName(name, typeParamCount, default));
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

        public ClassMemberFuncDeclSymbol? GetMemberFunc(M.Name name, int typeParamCount, ImmutableArray<M.FuncParamId> paramIds)
        {
            return funcDict.Get(new DeclSymbolNodeName(name, typeParamCount, paramIds));
        }

        public ImmutableArray<ClassMemberFuncDeclSymbol> GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        public ImmutableArray<ClassMemberFuncDeclSymbol> GetMemberFuncs()
        {
            return memberFuncs;
        }

        public ImmutableArray<ClassMemberTypeDeclSymbol> GetMemberTypes()
        {
            return memberTypes;
        }

        public int GetMemberVarCount()
        {
            return memberVars.Length;
        }

        public ClassMemberVarDeclSymbol GetMemberVar(int index)
        {
            return memberVars[index];
        }

        public M.Name GetName()
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
            return containerHolder.GetValue().GetOuterDeclNode();
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return memberTypes.AsEnumerable().OfType<IDeclSymbolNode>()
                .Concat(memberFuncs.AsEnumerable())
                .Concat(memberVars.AsEnumerable());
        }
       
        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClass(this);
        }

        public ClassMemberVarDeclSymbol? GetMemberVar(M.Name name)
        {
            foreach(var memberVar in memberVars)
            {
                if (name.Equals(memberVar.GetName()))
                    return memberVar;
            }

            return null;
        }

        public M.AccessModifier GetAccessModifier()
        {
            return containerHolder.GetValue().GetAccessModifier();
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
