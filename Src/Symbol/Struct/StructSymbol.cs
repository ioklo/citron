using System;
using System.Collections.Generic;
using Citron.Infra;
using Citron.Collections;

using Pretune;
using System.Diagnostics;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class StructSymbol : ITypeSymbol, ICyclicEqualityComparableClass<StructSymbol>
    {
        SymbolFactory symbolFactory;
        ISymbolNode outer;

        StructDeclSymbol decl;
        ImmutableArray<IType> typeArgs;

        TypeEnv typeEnv;

        internal StructSymbol(SymbolFactory symbolFactory, ISymbolNode outer, StructDeclSymbol structDecl, ImmutableArray<IType> typeArgs)
        {
            this.symbolFactory = symbolFactory;
            this.outer = outer;
            this.decl = structDecl;
            this.typeArgs = typeArgs;

            var outerTypeEnv = outer.GetTypeEnv();
            this.typeEnv = outerTypeEnv.AddTypeArgs(typeArgs);
        }

        SymbolQueryResult? QueryMember_Type(Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<SymbolQueryResult>();
            var nodeName = new DeclSymbolNodeName(memberName, typeParamCount, default);
            foreach (var memberTypeDecl in decl.GetMemberTypes())
            {
                // 이름이 같고, 타입 파라미터 개수가 같다면
                if (nodeName.Equals(memberTypeDecl.GetNodeName()))
                {
                    var symbolQueryResult = SymbolQueryResultBuilder.Build(memberTypeDecl, this, symbolFactory);
                    candidates.Add(symbolQueryResult);
                }
            }

            return candidates.MakeSymbolQueryResult();
        }

        public int GetMemberVarCount()
        {
            return decl.GetMemberVarCount();
        }

        public StructMemberVarSymbol GetMemberVar(int index)
        {
            var memberVarDSymbol = decl.GetMemberVar(index);
            return symbolFactory.MakeStructMemberVar(this, memberVarDSymbol);
        }

        public StructMemberVarSymbol? GetMemberVar(Name name)
        {
            var memberVarDSymbol = decl.GetMemberVar(name);
            if (memberVarDSymbol == null) return null;

            return symbolFactory.MakeStructMemberVar(this, memberVarDSymbol);
        }

        SymbolQueryResult? QueryMember_Func(Name memberName, int typeParamCount)
        {
            var funcsBuilder = ImmutableArray.CreateBuilder<DeclAndConstructor<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>>();
            
            foreach (var memberFuncDecl in decl.GetFuncs())
            {
                var funcName = memberFuncDecl.GetNodeName();

                if (memberName.Equals(funcName.Name) &&
                    typeParamCount <= funcName.TypeParamCount)
                {
                    var dac = new DeclAndConstructor<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>(
                        memberFuncDecl, 
                        typeArgs => symbolFactory.MakeStructMemberFunc(this, memberFuncDecl, typeArgs)
                    );

                    funcsBuilder.Add(dac);
                }
            }

            // 여러개 있을 수 있기때문에 MultipleCandidates를 리턴하지 않는다
            if (funcsBuilder.Count == 0)
                return null;

            return new SymbolQueryResult.StructMemberFuncs(funcsBuilder.ToImmutable());
            // new NestedItemValueOuter(this), funcsBuilder.ToImmutable(), bHaveInstance);
        }

        SymbolQueryResult? QueryMember_Var(Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<SymbolQueryResult>();

            int memberVarCount = decl.GetMemberVarCount();            

            for (int i = 0; i < memberVarCount; i++)
            {
                var memberVar = decl.GetMemberVar(i);

                if (memberVar.GetName().Equals(memberName))
                {
                    candidates.Add(new SymbolQueryResult.StructMemberVar(symbolFactory.MakeStructMemberVar(this, memberVar)));
                }
            }

            return candidates.MakeSymbolQueryResult();
        }

        public StructType? GetBaseStructType()
        {
            var baseType = decl.GetBaseStructType();
            if (baseType == null) return null;

            var typeEnv = GetTypeEnv();
            return baseType.Apply(typeEnv);
        }

        SymbolQueryResult? ISymbolNode.QueryMember(Name memberName, int typeParamCount)
            => QueryMember(memberName, typeParamCount);

        public SymbolQueryResult? QueryMember(Name memberName, int typeParamCount)
        {
            // TODO: caching
            var results = ImmutableArray.CreateBuilder<SymbolQueryResult>();

            // error, notfound, found
            var typeResult = QueryMember_Type(memberName, typeParamCount);
            if (typeResult is SymbolQueryResult.MultipleCandidatesError) return typeResult;
            if (typeResult != null) results.Add(typeResult);

            // error, notfound, found
            var funcResult = QueryMember_Func(memberName, typeParamCount);
            if (funcResult is SymbolQueryResult.MultipleCandidatesError) return funcResult;
            if (funcResult != null) results.Add(funcResult);

            // error, notfound, found
            if (typeParamCount == 0)
            {
                var varResult = QueryMember_Var(memberName, typeParamCount);
                if (varResult is SymbolQueryResult.MultipleCandidatesError) return varResult;
                if (varResult != null) results.Add(varResult);
            }

            if (1 < results.Count)
                return new SymbolQueryResult.MultipleCandidatesError(results.ToImmutable());

            if (results.Count == 0)
                return null;

            return results[0];
        }

        public IType? GetMemberType(Name memberName, ImmutableArray<IType> typeArgs)
        {
            // TODO: caching
            foreach (var memberType in decl.GetMemberTypes())
            {
                var typeName = memberType.GetNodeName();

                if (memberName.Equals(typeName.Name) && typeName.TypeParamCount == typeArgs.Length)
                    return SymbolInstantiator.Instantiate(symbolFactory, this, memberType, typeArgs).MakeType();
            }

            return null;
        }

        public StructSymbol Apply(TypeEnv typeEnv)
        {
            // [X<00T>.Y<10U>.Z<20V>].Apply([00 => int, 10 => short, 20 => string])

            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));

            return symbolFactory.MakeStruct(appliedOuter, decl, appliedTypeArgs);
        }

        IType ITypeSymbol.MakeType()
        {
            return new StructType(this);
        }

        public StructConstructorSymbol? GetTrivialConstructor()
        {
            var constructorInfo = decl.GetTrivialConstructor();
            if (constructorInfo == null) return null;

            return symbolFactory.MakeStructConstructor(this, constructorInfo);
        }

        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();

        public ITypeDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public IType GetTypeArg(int index)
        {
            return typeArgs[index];
        }

        public int GetConstructorCount()
        {
            return decl.GetConstructorCount();
        }

        public StructConstructorSymbol GetConstructor(int index)
        {
            var constructorDecl = decl.GetConstructor(index);
            return symbolFactory.MakeStructConstructor(this, constructorDecl);
        }

        public StructDeclSymbol GetDecl()
        {
            return decl;
        }

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is StructSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ITypeSymbol>.CyclicEquals(ITypeSymbol other, ref CyclicEqualityCompareContext context)
            => other is StructSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<StructSymbol>.CyclicEquals(StructSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(StructSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!context.CompareClass(decl, other.decl))
                return false;

            if (!typeArgs.CyclicEqualsClassItem(ref typeArgs, ref context))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(outer), outer);
            context.SerializeRef(nameof(decl), decl);
            context.SerializeRefArray(nameof(typeArgs), typeArgs);
        }

        void ISymbolNode.Accept<TVisitor>(ref TVisitor visitor)
        {
            visitor.VisitStruct(this);
        }

        void ITypeSymbol.Accept<TTypeSymbolVisitor>(ref TTypeSymbolVisitor visitor)
        {
            visitor.VisitStruct(this);
        }
    }
}
