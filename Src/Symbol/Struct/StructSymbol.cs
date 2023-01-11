using System;
using System.Collections.Generic;
using Citron.Infra;
using Citron.Collections;

using Pretune;
using System.Diagnostics;

namespace Citron.Symbol
{   
    public class StructSymbol : ITypeSymbol, ICyclicEqualityComparableClass<StructSymbol>
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

        SymbolQueryResult QueryMember_Type(Name memberName, int typeParamCount)
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

            var result = candidates.GetUniqueResult();
            if (result.IsFound(out var sqr))
                return sqr;
            else if (result.IsMultipleError())
                return SymbolQueryResults.Error.MultipleCandidates;
            else if (result.IsNotFound())
                return SymbolQueryResults.NotFound;

            throw new UnreachableCodeException();
        }

        public int GetMemberVarCount()
        {
            return decl.GetMemberVarCount();
        }

        public StructMemberVarSymbol GetMemberVar(int index)
        {
            var memberVarDecl = decl.GetMemberVar(index);
            return symbolFactory.MakeStructMemberVar(this, memberVarDecl);
        }

        SymbolQueryResult QueryMember_Func(Name memberName, int typeParamCount)
        {
            var funcsBuilder = ImmutableArray.CreateBuilder<DeclAndConstructor<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>>();

            bool bHaveInstance = false;
            bool bHaveStatic = false;
            foreach (var memberFuncDecl in decl.GetFuncs())
            {
                var funcName = memberFuncDecl.GetNodeName();

                if (memberName.Equals(funcName.Name) &&
                    typeParamCount <= funcName.TypeParamCount)
                {
                    bool bStaticFunc = memberFuncDecl.IsStatic();

                    bHaveInstance |= !bStaticFunc;
                    bHaveStatic |= bStaticFunc;

                    var dac = new DeclAndConstructor<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>(
                        memberFuncDecl, 
                        typeArgs => symbolFactory.MakeStructMemberFunc(this, memberFuncDecl, typeArgs)
                    );

                    funcsBuilder.Add(dac);
                }
            }

            // 여러개 있을 수 있기때문에 MultipleCandidates를 리턴하지 않는다
            if (funcsBuilder.Count == 0)
                return SymbolQueryResults.NotFound;

            // 둘다 가지고 있으면 안된다
            if (bHaveInstance && bHaveStatic)
                return SymbolQueryResults.Error.MultipleCandidates;

            return new SymbolQueryResult.StructMemberFuncs(funcsBuilder.ToImmutable());
            // new NestedItemValueOuter(this), funcsBuilder.ToImmutable(), bHaveInstance);
        }

        SymbolQueryResult QueryMember_Var(Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<SymbolQueryResult.StructMemberVar>();

            int memberVarCount = decl.GetMemberVarCount();            

            for (int i = 0; i < memberVarCount; i++)
            {
                var memberVar = decl.GetMemberVar(i);

                if (memberVar.GetName().Equals(memberName))
                {
                    candidates.Add(new SymbolQueryResult.StructMemberVar(symbolFactory.MakeStructMemberVar(this, memberVar)));
                }
            }
            

            var result = candidates.GetUniqueResult();
            if (result.IsFound(out var structMemberVar))
            {
                // 변수를 찾았는데 타입 아규먼트가 있다면 에러
                if (typeParamCount != 0)
                    return SymbolQueryResults.Error.VarWithTypeArg;

                return structMemberVar;
            }

            else if (result.IsMultipleError())
                return SymbolQueryResults.Error.MultipleCandidates;

            else if (result.IsNotFound())
                return SymbolQueryResults.NotFound;

            throw new UnreachableCodeException();
        }

        public StructType? GetBaseType()
        {
            var baseType = decl.GetBaseStruct();
            if (baseType == null) return null;

            var typeEnv = GetTypeEnv();
            return baseType.Apply(typeEnv);
        }

        public SymbolQueryResult QueryMember(Name memberName, int typeParamCount)
        {
            // TODO: caching
            var results = new List<SymbolQueryResult.Valid>();

            // error, notfound, found
            var typeResult = QueryMember_Type(memberName, typeParamCount);
            if (typeResult is SymbolQueryResult.Error) return typeResult;
            if (typeResult is SymbolQueryResult.Valid typeMemberResult) results.Add(typeMemberResult);

            // error, notfound, found
            var funcResult = QueryMember_Func(memberName, typeParamCount);
            if (funcResult is SymbolQueryResult.Error) return funcResult;
            if (funcResult is SymbolQueryResult.Valid funcMemberResult) results.Add(funcMemberResult);

            // error, notfound, found
            var varResult = QueryMember_Var(memberName, typeParamCount);
            if (varResult is SymbolQueryResult.Error) return varResult;
            if (varResult is SymbolQueryResult.Valid varMemberResult) results.Add(varMemberResult);

            if (1 < results.Count)
                return SymbolQueryResults.Error.MultipleCandidates;

            if (results.Count == 0)
                return SymbolQueryResults.NotFound;

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

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitStruct(this);
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

        public void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ISymbolNodeVisitor
        {
            visitor.VisitStruct(this);
        }
    }
}
