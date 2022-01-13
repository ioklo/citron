using System;
using System.Collections.Generic;
using Gum.Infra;
using Gum.Collections;

using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    public partial class StructSymbol : ITypeSymbolNode
    {
        SymbolFactory symbolFactory;
        ISymbolNode outer;

        StructDeclSymbol decl;
        ImmutableArray<ITypeSymbolNode> typeArgs;

        TypeEnv typeEnv;

        internal StructSymbol(SymbolFactory symbolFactory, ISymbolNode outer, StructDeclSymbol structDecl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            this.symbolFactory = symbolFactory;            
            this.outer = outer;
            this.decl = structDecl;
            this.typeArgs = typeArgs;

            var outerTypeEnv = outer.GetTypeEnv();
            this.typeEnv = outerTypeEnv.AddTypeArgs(typeArgs);
        }

        MemberQueryResult GetMember_Type(M.Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<MemberQueryResult.Valid>();
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
                return MemberQueryResult.Error.MultipleCandidates.Instance;

            if (candidates.IsEmpty)
                return MemberQueryResult.NotFound.Instance;

            throw new UnreachableCodeException();
        }
        
        MemberQueryResult GetMember_Func(M.Name memberName, int typeParamCount)
        {
            var funcsBuilder = ImmutableArray.CreateBuilder<Func<ImmutableArray<ITypeSymbolNode>, StructMemberFuncSymbol>>();

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
                return MemberQueryResult.NotFound.Instance;

            // 둘다 가지고 있으면 안된다
            if (bHaveInstance && bHaveStatic)
                return MemberQueryResult.Error.MultipleCandidates.Instance;

            return new MemberQueryResult.StructMemberFuncs(funcsBuilder.ToImmutable()); 
            // new NestedItemValueOuter(this), funcsBuilder.ToImmutable(), bHaveInstance);
        }
        
        MemberQueryResult GetMember_Var(M.Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<MemberQueryResult.StructMemberVar>();

            foreach (var memberVar in decl.GetMemberVars())
                if (memberVar.GetName().Equals(memberName))
                {
                    candidates.Add(new MemberQueryResult.StructMemberVar(symbolFactory.MakeStructMemberVar(this, memberVar)));
                }

            var result = candidates.GetSingle();
            if (result != null)
            {
                // 변수를 찾았는데 타입 아규먼트가 있다면 에러
                if (typeParamCount != 0)
                    return MemberQueryResult.Error.VarWithTypeArg.Instance;

                return result;
            }

            if (candidates.HasMultiple)
                return MemberQueryResult.Error.MultipleCandidates.Instance;

            if (candidates.IsEmpty)
                return MemberQueryResult.NotFound.Instance;

            throw new UnreachableCodeException();
        }

        public StructSymbol? GetBaseType()
        {
            var baseType = decl.GetBaseStruct();
            if (baseType == null) return null;

            var typeEnv = GetTypeEnv();
            return baseType.Apply(typeEnv);
        }

        public MemberQueryResult GetMember(M.Name memberName, int typeParamCount)
        {
            if (memberName.Equals(M.Name.Constructor))
            {
                // TODO: Constructor를 GetMember로 가져오는것이 맞는가??
                var constructors = ImmutableArray.CreateBuilder<StructConstructorSymbol>();
                
                foreach(var constructorDecl in decl.GetConstructors())
                    constructors.Add(symbolFactory.MakeStructConstructor(this, constructorDecl));

                return new MemberQueryResult.StructConstructors(constructors.ToImmutable());
            }

            // TODO: caching
            var results = new List<MemberQueryResult.Valid>();

            // error, notfound, found
            var typeResult = GetMember_Type(memberName, typeParamCount);
            if (typeResult is MemberQueryResult.Error) return typeResult;
            if (typeResult is MemberQueryResult.Valid typeMemberResult) results.Add(typeMemberResult);

            // error, notfound, found
            var funcResult = GetMember_Func(memberName, typeParamCount);
            if (funcResult is MemberQueryResult.Error) return funcResult;
            if (funcResult is MemberQueryResult.Valid funcMemberResult) results.Add(funcMemberResult);

            // error, notfound, found
            var varResult = GetMember_Var(memberName, typeParamCount);
            if (varResult is MemberQueryResult.Error) return varResult;
            if (varResult is MemberQueryResult.Valid varMemberResult) results.Add(varMemberResult);            

            if (1 < results.Count)
                return MemberQueryResult.Error.MultipleCandidates.Instance;

            if (results.Count == 0)
                return MemberQueryResult.NotFound.Instance;

            return results[0];
        }

        public ITypeSymbolNode? GetMemberType(M.Name memberName, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            // TODO: caching
            foreach (var memberType in decl.GetMemberTypes())
            {
                var typeName = memberType.GetNodeName();

                if (memberName.Equals(typeName.Name) && typeName.TypeParamCount == typeArgs.Length)
                    return MemberTypeInstantiator.Instantiate(symbolFactory, this, memberType, typeArgs);
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
        
        public R.Path.Nested MakeRPath()
        {
            var rname = RItemFactory.MakeName(decl.GetName());
            var rtypeArgs = RItemFactory.MakeRTypes(typeArgs);

            return new R.Path.Nested(outer.MakeRPath(), rname, new R.ParamHash(decl.GetTypeParamCount(), default), rtypeArgs);
        }

        public R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
        {
            return new R.StructMemberLoc(instance, member);
        }

        public StructConstructorSymbol? GetTrivialConstructor()
        {
            var constructorInfo = decl.GetTrivialConstructor();
            if (constructorInfo == null) return null;

            return symbolFactory.MakeStructConstructor(this, constructorInfo);
        }

        public int GetTotalTypeParamCount()
        {
            return decl.GetTotalTypeParamCount();
        }

        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        ITypeSymbolNode ITypeSymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        public ISymbolNode? GetOuter()
        {
            return outer;
        }
        
        R.Path.Normal ISymbolNode.MakeRPath() => MakeRPath();
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        ITypeDeclSymbolNode ITypeSymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();

        public StructDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            return typeArgs;
        }
    }
}
