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
    public partial class StructDeclSymbol : ITypeDeclSymbol
    {
        IHolder<IDeclSymbolNode> outerHolder;
        AccessModifier accessModifier;

        Name name;
        ImmutableArray<TypeVarDeclSymbol> typeParams;
        IHolder<StructSymbol?> baseStructHolder;
        ImmutableArray<ITypeDeclSymbol> memberTypesExceptTypeVars;
        ImmutableArray<StructMemberFuncDeclSymbol> funcDecls;        
        ImmutableArray<StructMemberVarDeclSymbol> varDecls;

        IHolder<ImmutableArray<StructConstructorDeclSymbol>> constructorsHolder;
        IHolder<StructConstructorDeclSymbol?> trivialConstructorHolder;

        TypeDict typeDict;
        FuncDict<StructMemberFuncDeclSymbol> funcDict;

        public StructDeclSymbol(
            IHolder<IDeclSymbolNode> outerHolder,
            AccessModifier accessModifier,
            Name name, ImmutableArray<TypeVarDeclSymbol> typeParams,
            IHolder<StructSymbol?> baseStructHolder,
            ImmutableArray<ITypeDeclSymbol> memberTypesExceptTypeVars,
            ImmutableArray<StructMemberFuncDeclSymbol> memberFuncs,            
            ImmutableArray<StructMemberVarDeclSymbol> memberVars,
            IHolder<ImmutableArray<StructConstructorDeclSymbol>> constructorsHolder,
            IHolder<StructConstructorDeclSymbol?> trivialConstructorHolder)
        {
            this.outerHolder = outerHolder;
            this.accessModifier = accessModifier;
            this.name = name;
            this.typeParams = typeParams;
            this.baseStructHolder = baseStructHolder;
            this.memberTypesExceptTypeVars = memberTypesExceptTypeVars;
            this.funcDecls = memberFuncs;            
            this.varDecls = memberVars;
            this.constructorsHolder = constructorsHolder;
            this.trivialConstructorHolder = trivialConstructorHolder;

            this.typeDict = TypeDict.Build(typeParams, memberTypesExceptTypeVars);
            this.funcDict = FuncDict.Build(memberFuncs);
        }

        // include type parameters
        public ImmutableArray<ITypeDeclSymbol> GetMemberTypes()
        {
            var builder = ImmutableArray.CreateBuilder<ITypeDeclSymbol>(typeParams.Length + memberTypesExceptTypeVars.Length);
            builder.AddRange(typeParams.AsEnumerable());
            builder.AddRange(memberTypesExceptTypeVars.AsEnumerable());
            return builder.MoveToImmutable();
        }

        public Name GetName()
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

        public StructMemberFuncDeclSymbol? GetFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return funcDict.Get(new DeclSymbolNodeName(name, typeParamCount, paramIds));
        }

        public ImmutableArray<StructMemberFuncDeclSymbol> GetFuncs(Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        public ITypeDeclSymbol? GetType(Name name, int typeParamCount)
        {
            return typeDict.Get(new DeclSymbolNodeName(name, typeParamCount, default));
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
            return outerHolder.GetValue();
        }
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return typeDict.GetEnumerable().OfType<IDeclSymbolNode>()
                .Concat(funcDict.GetEnumerable())
                .Concat(varDecls.AsEnumerable());
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

        public AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }
    }
}