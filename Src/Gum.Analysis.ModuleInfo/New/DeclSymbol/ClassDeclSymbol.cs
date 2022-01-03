using Gum.Analysis;
using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // StructDel? outer = null;
    // var innerClass = new ClassDeclSymbol(() => outer!, ....);
    // outer = new StructDecl(innerClass);

    [ImplementIEquatable]
    public partial class ClassDeclSymbol : ITypeDeclSymbolNode
    {
        IHolder<IDeclSymbolNode> outer;

        M.Name name;
        ImmutableArray<string> typeParams;
        IHolder<ImmutableArray<InterfaceSymbol>> interfaceTypes;

        ImmutableArray<ClassMemberTypeDeclSymbol> typeDecls;
        ImmutableArray<ClassMemberFuncDeclSymbol> funcDecls;
        IHolder<ImmutableArray<ClassConstructorDeclSymbol>> constructorDecls; // 추가적으로 생성되는 것들 때문에 이렇게 했는데 약간 크게 잡은 경향이 있다
        ImmutableArray<ClassMemberVarDeclSymbol> varDecls;

        // Class선언 시점 typeEnv를 적용한 baseClass
        IHolder<ClassSymbol> baseClass;        
        IHolder<ClassConstructorDeclSymbol?> trivialConstructor;

        ClassDeclSymbolBuildState state;

        [ExcludeComparison]
        TypeDict<ClassMemberTypeDeclSymbol> typeDict;

        [ExcludeComparison]
        FuncDict<ClassMemberFuncDeclSymbol> funcDict;
        
        public ClassDeclSymbol(
            IHolder<IDeclSymbolNode> outer,
            M.Name name, ImmutableArray<string> typeParams,
            IHolder<ClassSymbol> baseClass,
            IHolder<ImmutableArray<InterfaceSymbol>> interfaceTypes,
            ImmutableArray<ClassMemberTypeDeclSymbol> typeDecls,
            ImmutableArray<ClassMemberFuncDeclSymbol> funcDecls,
            ImmutableArray<ClassMemberVarDeclSymbol> varDecls,
            IHolder<ImmutableArray<ClassConstructorDeclSymbol>> constructorDecls,
            IHolder<ClassConstructorDeclSymbol?> trivialConstructor)
        {
            this.outer = outer;
            this.name = name;
            this.typeParams = typeParams;
            this.baseClass = baseClass;
            this.interfaceTypes = interfaceTypes;
            this.typeDecls = typeDecls;
            this.funcDecls = funcDecls;            
            this.varDecls = varDecls;

            this.constructorDecls = constructorDecls;
            this.trivialConstructor = trivialConstructor;
            this.state = ClassDeclSymbolBuildState.BeforeSetBaseAndBuildTrivialConstructor;

            this.typeDict = TypeDict.Build(typeDecls);
            this.funcDict = FuncDict.Build(funcDecls);
        }
        
        // trivial constructor를 하려면
        public void EnsureSetBaseAndBuildTrivialConstructor(IQueryModuleTypeDecl query) // throws InvalidOperationException
        {
            if (state == ClassDeclSymbolBuildState.Completed) return;
            if (state == ClassDeclSymbolBuildState.DuringSetBaseAndBuildTrivialConstructor)
                throw new InvalidOperationException();

            Debug.Assert(state == ClassDeclSymbolBuildState.BeforeSetBaseAndBuildTrivialConstructor);
            state = ClassDeclSymbolBuildState.DuringSetBaseAndBuildTrivialConstructor;

            // base 클래스가 있다면
            if (baseClassId != null)
            {
                // class B<T>
                // {
                //     public B(T t) { } // trivial
                // }
                // class C : B<int> { }

                baseClass = query.GetClass(baseClassId);
            }

            var baseTrivialConstructor = baseClass?.GetTrivialConstructor();

            // baseClass가 있고, TrivialConstructor가 없는 경우 => 안 만들고 진행
            // baseClass가 있고, TrivialConstructor가 있는 경우 => 진행
            // baseClass가 없는 경우 => 없이 만들고 진행 
            if (baseTrivialConstructor != null || baseClassId == null)
            {
                // 같은 인자의 생성자가 없으면 Trivial을 만든다
                if (SymbolMisc.GetConstructorHasSameParamWithTrivial(baseTrivialConstructor, constructorDecls, varDecls) == null)
                {
                    trivialConstructor = SymbolMisc.MakeTrivialConstructorDecl(this, baseTrivialConstructor, varDecls);
                    constructorDecls = constructorDecls.Add(trivialConstructor);
                }
            }

            state = ClassDeclSymbolBuildState.Completed;
        }

        public ClassConstructorDeclSymbol? GetDefaultConstructorDecl()
        {
            foreach (var decl in constructorDecls)
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
            Debug.Assert(state == ClassDeclSymbolBuildState.Completed);
            return trivialConstructor;
        }        

        // Info자체에는 environment가 없으므로, typeEnv가 있어야
        public ClassSymbol? GetBaseClass()
        {
            Debug.Assert(state == ClassDeclSymbolBuildState.Completed);
            return baseClass;
        }
        
        public ImmutableArray<ClassConstructorDeclSymbol> GetConstructors()
        {
            return constructorDecls;
        }

        public ClassMemberFuncDeclSymbol? GetMemberFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return funcDict.Get(new DeclSymbolNodeName(name, typeParamCount, paramTypes));
        }

        public ImmutableArray<ClassMemberFuncDeclSymbol> GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        public ImmutableArray<ClassMemberFuncDeclSymbol> GetMemberFuncs()
        {
            return funcDecls;
        }

        public ImmutableArray<ClassMemberTypeDeclSymbol> GetMemberTypes()
        {
            return typeDecls;
        }

        public ImmutableArray<ClassMemberVarDeclSymbol> GetMemberVars()
        {
            return varDecls;
        }

        public M.Name GetName()
        {
            return name;
        }
        
        public void Apply(ITypeDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitClassDecl(this);
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            var nodeName = new DeclSymbolNodeName(name, typeParamCount, paramTypes);

            if (paramTypes.IsEmpty)
            {
                if (typeParamCount == 0)
                {
                    foreach (var varDecl in varDecls)
                        if (varDecl.GetName().Equals(name))
                            return varDecl;
                }

                var typeDecl = typeDict.Get(nodeName);
                if (typeDecl != null)
                    return typeDecl;
            }

            var funcDecl = funcDict.Get(nodeName);
            if (funcDecl != null)
                return funcDecl;

            return null;
        }
    }
}
