using Gum.Analysis;
using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using M = Gum.CompileTime;

using static Gum.CompileTime.ItemPathExtensions;
using Gum.Infra;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    public partial class StructDeclSymbol : ITypeDeclSymbolNode
    {
        IHolder<IDeclSymbolNode> outer;

        M.Name name;
        ImmutableArray<string> typeParams;
        IHolder<StructSymbol> baseStructHolder;
        ImmutableArray<StructMemberTypeDeclSymbol> typeDecls;
        ImmutableArray<StructMemberFuncDeclSymbol> funcDecls;        
        ImmutableArray<StructMemberVarDeclSymbol> varDecls;

        IHolder<ImmutableArray<StructConstructorDeclSymbol>> constructorsHolder;
        IHolder<StructConstructorDeclSymbol?> trivialConstructorHolder;

        TypeDict<StructMemberTypeDeclSymbol> typeDict;
        FuncDict<StructMemberFuncDeclSymbol> funcDict;

        // state따라 valid 하지 않을수 있다
        StructSymbol? baseStruct;
        StructConstructorDeclSymbol? trivialConstructor;

        StructSymbolBuildState state;

        public StructDeclSymbol(
            IHolder<IDeclSymbolNode> outer,
            M.Name name, ImmutableArray<string> typeParams,
            IHolder<StructSymbol> baseStructHolder,
            ImmutableArray<StructMemberTypeDeclSymbol> typeDecls,
            ImmutableArray<StructMemberFuncDeclSymbol> funcDecls,            
            ImmutableArray<StructMemberVarDeclSymbol> memberVars,
            IHolder<ImmutableArray<StructConstructorDeclSymbol>> constructorsHolder,
            IHolder<StructConstructorDeclSymbol?> trivialConstructorHolder)
        {
            this.outer = outer;
            this.name = name;
            this.typeParams = typeParams;
            this.baseStructHolder = baseStructHolder;
            this.typeDecls = typeDecls;
            this.funcDecls = funcDecls;            
            this.varDecls = memberVars;
            this.constructorsHolder = constructorsHolder;
            this.trivialConstructorHolder = trivialConstructorHolder;

            this.typeDict = TypeDict.Build(typeDecls);
            this.funcDict = FuncDict.Build(funcDecls);

            this.baseStruct = null;
            this.trivialConstructor = null;
            this.state = StructSymbolBuildState.BeforeSetBaseAndBuildTrivialConstructor;
        }

        public ImmutableArray<StructMemberTypeDeclSymbol> GetMemberTypes()
        {
            return typeDecls;
        }

        public M.Name GetName()
        {
            return name;
        }

        // TODO: InternalModuleInfoBuilder로 가야합
        //public void EnsureSetBaseAndBuildTrivialConstructor(IQueryModuleTypeDecl query) // throws InvalidOperation
        //{
        //    if (state == StructSymbolBuildState.Completed) return;

        //    if (state == StructSymbolBuildState.DuringSetBaseAndBuildTrivialConstructor)
        //        throw new InvalidOperationException();

        //    Debug.Assert(state == StructSymbolBuildState.BeforeSetBaseAndBuildTrivialConstructor);
        //    state = StructSymbolBuildState.DuringSetBaseAndBuildTrivialConstructor;

        //    StructSymbol? baseStruct = null;
        //    if (baseStructId != null)
        //    {
        //        baseStruct = query.GetStruct(baseStructId);
        //    }

        //    var baseTrivialConstructor = baseStruct?.GetTrivialConstructor();

        //    // baseClass가 있고, TrivialConstructor가 없는 경우 => 안 만들고 진행
        //    // baseClass가 있고, TrivialConstructor가 있는 경우 => 진행
        //    // baseClass가 없는 경우 => 없이 만들고 진행 
        //    if (baseTrivialConstructor != null || baseStruct == null)
        //    {
        //        // 같은 인자의 생성자가 없으면 Trivial을 만든다
        //        if (SymbolMisc.GetConstructorHasSameParamWithTrivial(baseTrivialConstructor, constructorDecls, varDecls) == null)
        //        {
        //            trivialConstructor = SymbolMisc.MakeTrivialConstructorDecl(this, baseTrivialConstructor, varDecls);
        //            constructorDecls = constructorDecls.Add(trivialConstructor);
        //        }
        //    }

        //    state = StructSymbolBuildState.Completed;
        //    return;
        //}

        public ImmutableArray<StructMemberFuncDeclSymbol> GetMemberFuncs()
        {
            return funcDecls;
        }

        public StructConstructorDeclSymbol? GetTrivialConstructor()
        {
            return trivialConstructor;
        }

        public StructSymbol? GetBaseStruct()
        {
            Debug.Assert(state == StructSymbolBuildState.Completed);
            return baseStruct;
        }

        public ImmutableArray<StructConstructorDeclSymbol> GetConstructors()
        {
            return constructorsHolder.GetValue();
        }

        public StructMemberFuncDeclSymbol? GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return funcDict.Get(new DeclSymbolNodeName(name, typeParamCount, paramTypes));
        }

        public ImmutableArray<StructMemberFuncDeclSymbol> GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }        

        public ImmutableArray<StructMemberVarDeclSymbol> GetMemberVars()
        {
            return varDecls;
        }

        public StructMemberTypeDeclSymbol? GetType(M.Name name, int typeParamCount)
        {
            return typeDict.Get(new DeclSymbolNodeName(name, typeParamCount, default));
        }

        public ImmutableArray<string> GetTypeParams()
        {
            return typeParams;
        }

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, default);
        }

        public void Apply(ITypeDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructDecl(this);
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