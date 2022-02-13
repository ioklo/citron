using System;
using System.Collections.Generic;
using System.Diagnostics;

using Citron.Analysis;
using Citron.Collections;
using Citron.Infra;

using Pretune;
using M = Citron.CompileTime;

namespace Citron.Analysis
{
    [ImplementIEquatable]
    public partial class StructDeclSymbol : ITypeDeclSymbol
    {
        IHolder<ITypeDeclSymbolContainer> containerHolder;

        M.Name name;
        ImmutableArray<string> typeParams;
        IHolder<StructSymbol?> baseStructHolder;
        ImmutableArray<StructMemberTypeDeclSymbol> typeDecls;
        ImmutableArray<StructMemberFuncDeclSymbol> funcDecls;        
        ImmutableArray<StructMemberVarDeclSymbol> varDecls;

        IHolder<ImmutableArray<StructConstructorDeclSymbol>> constructorsHolder;
        IHolder<StructConstructorDeclSymbol?> trivialConstructorHolder;

        TypeDict<StructMemberTypeDeclSymbol> typeDict;
        FuncDict<StructMemberFuncDeclSymbol> funcDict;

        public StructDeclSymbol(
            IHolder<ITypeDeclSymbolContainer> containerHolder,
            M.Name name, ImmutableArray<string> typeParams,
            IHolder<StructSymbol?> baseStructHolder,
            ImmutableArray<StructMemberTypeDeclSymbol> typeDecls,
            ImmutableArray<StructMemberFuncDeclSymbol> funcDecls,            
            ImmutableArray<StructMemberVarDeclSymbol> memberVars,
            IHolder<ImmutableArray<StructConstructorDeclSymbol>> constructorsHolder,
            IHolder<StructConstructorDeclSymbol?> trivialConstructorHolder)
        {
            this.containerHolder = containerHolder;
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
        }

        public ImmutableArray<StructMemberTypeDeclSymbol> GetMemberTypes()
        {
            return typeDecls;
        }

        public M.Name GetName()
        {
            return name;
        }

        public ImmutableArray<StructMemberFuncDeclSymbol> GetMemberFuncs()
        {
            return funcDecls;
        }

        public StructConstructorDeclSymbol? GetTrivialConstructor()
        {
            return trivialConstructorHolder.GetValue();
        }

        public StructSymbol? GetBaseStruct()
        {
            return baseStructHolder.GetValue();
        }

        public ImmutableArray<StructConstructorDeclSymbol> GetConstructors()
        {
            return constructorsHolder.GetValue();
        }

        public StructMemberFuncDeclSymbol? GetFunc(M.Name name, int typeParamCount, ImmutableArray<M.FuncParamId> paramIds)
        {
            return funcDict.Get(new DeclSymbolNodeName(name, typeParamCount, paramIds));
        }

        public ImmutableArray<StructMemberFuncDeclSymbol> GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
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

        public int GetMemberVarCount()
        {
            return varDecls.Length;
        }

        public StructMemberVarDeclSymbol GetMemberVar(int index)
        {
            return varDecls[index];
        }

        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitStruct(this);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return containerHolder.GetValue().GetOuterDeclNode();
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<M.FuncParamId> paramIds)
        {
            var nodeName = new DeclSymbolNodeName(name, typeParamCount, paramIds);

            if (paramIds.IsEmpty)
            {
                if (typeParamCount == 0)
                {
                    foreach (var varDecl in varDecls)
                        if (varDecl.GetName().Equals(name))
                            return varDecl;
                }

                var typeDecl = typeDict.Get(nodeName);
                if (typeDecl != null)
                    return typeDecl.GetNode();
            }

            var funcDecl = funcDict.Get(nodeName);
            if (funcDecl != null)
                return funcDecl;

            return null;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStruct(this);
        }

        public int GetConstructorCount()
        {
            return constructorsHolder.GetValue().Length;
        }

        public StructConstructorDeclSymbol GetConstructor(int index)
        {
            return constructorsHolder.GetValue()[index];
        }

        public M.AccessModifier GetAccessModifier()
        {
            return containerHolder.GetValue().GetAccessModifier();
        }
    }
}