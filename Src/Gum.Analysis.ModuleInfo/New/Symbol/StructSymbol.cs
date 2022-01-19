using System;
using System.Collections.Generic;
using Gum.Infra;
using Gum.Collections;

using M = Gum.CompileTime;
using Pretune;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    public partial class StructSymbol : ITypeSymbol
    {
        SymbolFactory symbolFactory;
        ISymbolNode outer;

        StructDeclSymbol decl;
        ImmutableArray<ITypeSymbol> typeArgs;

        TypeEnv typeEnv;

        internal StructSymbol(SymbolFactory symbolFactory, ISymbolNode outer, StructDeclSymbol structDecl, ImmutableArray<ITypeSymbol> typeArgs)
        {
            this.symbolFactory = symbolFactory;            
            this.outer = outer;
            this.decl = structDecl;
            this.typeArgs = typeArgs;

            var outerTypeEnv = outer.GetTypeEnv();
            this.typeEnv = outerTypeEnv.AddTypeArgs(typeArgs);
        }

        SymbolQueryResult QueryMember_Type(M.Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<SymbolQueryResult.Valid>();
            var resultFactory = new MemberQueryResultCandidatesBuilder(this, symbolFactory, candidates);

            var nodeName = new DeclSymbolNodeName(memberName, typeParamCount, default);
            foreach (var memberTypeDecl in decl.GetMemberTypes())
            {
                // 이름이 같고, 타입 파라미터 개수가 같다면
                if (nodeName.Equals(memberTypeDecl.GetNodeName()))
                {   
                    memberTypeDecl.Apply(resultFactory);
                }
            }

            var result = candidates.GetSingle();
            if (result != null)
                return result;

            if (candidates.HasMultiple)
                return SymbolQueryResult.Error.MultipleCandidates.Instance;

            if (candidates.IsEmpty)
                return SymbolQueryResult.NotFound.Instance;

            throw new UnreachableCodeException();
        }
        
        SymbolQueryResult QueryMember_Func(M.Name memberName, int typeParamCount)
        {
            var funcsBuilder = ImmutableArray.CreateBuilder<Func<ImmutableArray<ITypeSymbol>, StructMemberFuncSymbol>>();

            bool bHaveInstance = false;
            bool bHaveStatic = false;
            foreach (var memberFuncDecl in decl.GetMemberFuncs())
            {
                var funcName = memberFuncDecl.GetNodeName();

                if (memberName.Equals(funcName.Name) &&
                    typeParamCount <= funcName.TypeParamCount)
                {
                    bool bStaticFunc = memberFuncDecl.IsStatic();

                    bHaveInstance |= !bStaticFunc;
                    bHaveStatic |= bStaticFunc;
                    
                    funcsBuilder.Add(typeArgs => symbolFactory.MakeStructMemberFunc(this, memberFuncDecl, typeArgs));
                }
            }

            // 여러개 있을 수 있기때문에 MultipleCandidates를 리턴하지 않는다
            if (funcsBuilder.Count == 0)
                return SymbolQueryResult.NotFound.Instance;

            // 둘다 가지고 있으면 안된다
            if (bHaveInstance && bHaveStatic)
                return SymbolQueryResult.Error.MultipleCandidates.Instance;

            return new SymbolQueryResult.StructMemberFuncs(funcsBuilder.ToImmutable()); 
            // new NestedItemValueOuter(this), funcsBuilder.ToImmutable(), bHaveInstance);
        }
        
        SymbolQueryResult QueryMember_Var(M.Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<SymbolQueryResult.StructMemberVar>();

            foreach (var memberVar in decl.GetMemberVars())
                if (memberVar.GetName().Equals(memberName))
                {
                    candidates.Add(new SymbolQueryResult.StructMemberVar(symbolFactory.MakeStructMemberVar(this, memberVar)));
                }

            var result = candidates.GetSingle();
            if (result != null)
            {
                // 변수를 찾았는데 타입 아규먼트가 있다면 에러
                if (typeParamCount != 0)
                    return SymbolQueryResult.Error.VarWithTypeArg.Instance;

                return result;
            }

            if (candidates.HasMultiple)
                return SymbolQueryResult.Error.MultipleCandidates.Instance;

            if (candidates.IsEmpty)
                return SymbolQueryResult.NotFound.Instance;

            throw new UnreachableCodeException();
        }

        public StructSymbol? GetBaseType()
        {
            var baseType = decl.GetBaseStruct();
            if (baseType == null) return null;

            var typeEnv = GetTypeEnv();
            return baseType.Apply(typeEnv);
        }

        public SymbolQueryResult QueryMember(M.Name memberName, int typeParamCount)
        {
            if (memberName.Equals(M.Name.Constructor))
            {
                // TODO: Constructor를 GetMember로 가져오는것이 맞는가??
                var constructors = ImmutableArray.CreateBuilder<StructConstructorSymbol>();
                
                foreach(var constructorDecl in decl.GetConstructors())
                    constructors.Add(symbolFactory.MakeStructConstructor(this, constructorDecl));

                return new SymbolQueryResult.StructConstructors(constructors.ToImmutable());
            }

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
                return SymbolQueryResult.Error.MultipleCandidates.Instance;

            if (results.Count == 0)
                return SymbolQueryResult.NotFound.Instance;

            return results[0];
        }

        public ITypeSymbol? GetMemberType(M.Name memberName, ImmutableArray<ITypeSymbol> typeArgs)
        {
            // TODO: caching
            foreach (var memberType in decl.GetMemberTypes())
            {
                var typeName = memberType.GetNodeName();

                if (memberName.Equals(typeName.Name) && typeName.TypeParamCount == typeArgs.Length)
                    return SymbolInstantiator.Instantiate(symbolFactory, this, memberType, typeArgs);
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
        ITypeDeclSymbol ITypeSymbol.GetDeclSymbolNode() => GetDeclSymbolNode();

        public StructDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return typeArgs;
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitStruct(this);
        }
    }
}
