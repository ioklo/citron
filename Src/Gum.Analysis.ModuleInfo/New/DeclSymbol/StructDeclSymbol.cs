using Gum.Analysis;
using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using M = Gum.CompileTime;

using Gum.Infra;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    public partial class StructDeclSymbol : ITypeDeclSymbolNode
    {
        IHolder<IDeclSymbolNode> outer;

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
            IHolder<IDeclSymbolNode> outer,
            M.Name name, ImmutableArray<string> typeParams,
            IHolder<StructSymbol?> baseStructHolder,
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