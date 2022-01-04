using System;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;
using Gum.Collections;
using Gum.Infra;
using System.Collections.Generic;
using Gum.Analysis;

namespace Gum.Analysis
{
    // value
    [ImplementIEquatable]
    public partial class ClassSymbol : ITypeSymbolNode
    {
        SymbolFactory factory;
        ISymbolNode outer;
        ClassDeclSymbol decl;
        ImmutableArray<ITypeSymbolNode> typeArgs;

        TypeEnv typeEnv;

        internal ClassSymbol(SymbolFactory factory, ISymbolNode outer, ClassDeclSymbol decl, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
            this.typeArgs = typeArgs;

            var outerTypeEnv = outer.GetTypeEnv();
            this.typeEnv = outerTypeEnv.AddTypeArgs(typeArgs);
        }

        // from ISymbolNode
        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        // class X<T> { class Y<U> : Z<T>.B<T> }
        // X<int>.Y<bool> c; // c의 classTypeValue { [int, bool], X<>.Y<> }
        // baseof(c) //  classTypeValue { [int, int], Z<>.B<> } <- 여기에
        public ClassSymbol? GetBaseType() 
        {
            // 지금 속한 클래스의 타입 환경에 종속된 ClassSymbol를 돌려준다
            // X<T>.C<U> : B<U, T> => ClassSymbol(B, [TV(1), TV(0)])
            var baseClassTypeValue = decl.GetBaseClass();
            if (baseClassTypeValue == null) return null;

            // ClassSymbol가 X<int>.C<TV(4)>라면 TV(0)을 int로, TV(1)을 TV(4)로 치환한다
            // TV(4)는 x<int>.C<TV(4)>를 선언한 환경이다
            return baseClassTypeValue.Apply(typeEnv);
        }

        // except itself
        public bool IsBaseOf(ClassSymbol derivedType)
        {
            ClassSymbol? curBaseType = derivedType.GetBaseType();

            while(curBaseType != null)
            {
                if (Equals(curBaseType)) return true;
                curBaseType = curBaseType.GetBaseType();
            }

            return false;
        }
        
        public ClassSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeClass(appliedOuter, decl, appliedTypeArgs);
        }

        public R.Path.Nested MakeRPath()
        {
            var rname = RItemFactory.MakeName(decl.GetName());
            var rtypeArgs = RItemFactory.MakeRTypes(typeArgs);

            return new R.Path.Nested(outer.MakeRPath(), rname, new R.ParamHash(decl.GetTypeParamCount(), default), rtypeArgs);
        }

        public R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested memberPath)
        {
            return new R.ClassMemberLoc(instance, memberPath);
        }

        // 인자와 멤버변수가 1:1로 매칭되는 constructor를 의미한다. (base 클래스 포함). generate해야 한다. 미리 선언한 constructor는 trivial이 아니다
        public ClassConstructorSymbol? GetTrivialConstructor()
        {
            var constructorInfo = decl.GetTrivialConstructor();
            if (constructorInfo == null) return null;

            return factory.MakeClassConstructor(this, constructorInfo);
        }

        // 빈 constructor를 말한다 (Access 체크는 하지 않는다)
        public ClassConstructorSymbol? GetDefaultConstructor()
        {
            var defaultDecl = decl.GetDefaultConstructorDecl();
            if (defaultDecl == null)
                return null;

            return factory.MakeClassConstructor(this, defaultDecl);
        }
        
        // 멤버 쿼리 서비스
        MemberQueryResult GetMember_Type(M.Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<MemberQueryResult.Valid>();
            var candidatesBuilder = new MemberQueryResultCandidatesBuilder(this, factory, candidates);

            var nodeName = new DeclSymbolNodeName(memberName, typeParamCount, default);

            foreach (var memberTypeDecl in decl.GetMemberTypes())
            {
                // 이름이 같고, 타입 파라미터 개수가 같다면
                if (nodeName.Equals(memberTypeDecl.GetNodeName()))
                {
                    memberTypeDecl.Apply(candidatesBuilder);
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
            var builder = ImmutableArray.CreateBuilder<Func<ImmutableArray<ITypeSymbolNode>, ClassMemberFuncSymbol>>();

            bool bHaveInstance = false;
            bool bHaveStatic = false;
            foreach (var memberFunc in decl.GetMemberFuncs())
            {
                var funcName = memberFunc.GetNodeName();

                if (funcName.Name.Equals(memberName) &&
                    typeParamCount <= funcName.TypeParamCount)
                {
                    bool bStatic = memberFunc.IsStatic();

                    bHaveInstance |= !bStatic;
                    bHaveStatic |= bStatic;

                    builder.Add(typeArgs => factory.MakeClassMemberFunc(this, memberFunc, typeArgs));
                }
            }

            // 여러개 있을 수 있기때문에 MultipleCandidates를 리턴하지 않는다
            if (builder.Count == 0)
                return MemberQueryResult.NotFound.Instance;

            // 둘다 가지고 있으면 안된다
            if (bHaveInstance && bHaveStatic)
                return MemberQueryResult.Error.MultipleCandidates.Instance;

            return new MemberQueryResult.ClassMemberFuncs(builder.ToImmutable());
        }

        MemberQueryResult GetMember_Var(M.Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<ClassMemberVarSymbol>();

            foreach (var memberVar in decl.GetMemberVars())
                if (memberVar.GetName().Equals(memberName))
                {
                    candidates.Add(factory.MakeClassMemberVar(this, memberVar));
                }

            var result = candidates.GetSingle();
            if (result != null)
            {
                // 변수를 찾았는데 타입 아규먼트가 있다면 에러
                if (typeParamCount != 0)
                    return MemberQueryResult.Error.VarWithTypeArg.Instance;

                return new MemberQueryResult.ClassMemberVar(result);
            }

            if (candidates.HasMultiple)
                return MemberQueryResult.Error.MultipleCandidates.Instance;

            if (candidates.IsEmpty)
                return MemberQueryResult.NotFound.Instance;

            throw new UnreachableCodeException();
        }

        public MemberQueryResult GetMember(M.Name memberName, int typeParamCount)
        {
            if (memberName.Equals(M.Name.Constructor))
            {
                var constructorDecls = decl.GetConstructors();
                var builder = ImmutableArray.CreateBuilder<ClassConstructorSymbol>(constructorDecls.Length);

                foreach (var constructorDecl in decl.GetConstructors())
                    builder.Add(factory.MakeClassConstructor(this, constructorDecl));

                return new MemberQueryResult.ClassConstructors(builder.MoveToImmutable());
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
            {
                var baseTypeValue = GetBaseType();
                if (baseTypeValue == null)
                    return MemberQueryResult.NotFound.Instance;

                return baseTypeValue.GetMember(memberName, typeParamCount);
            }
            else
            {
                return results[0];
            }
        }

        public ITypeSymbolNode? GetMemberType(M.Name memberName, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            // TODO: caching
            foreach (var memberTypeDecl in decl.GetMemberTypes())
            {
                var typeName = memberTypeDecl.GetNodeName();

                if (typeName.Name.Equals(memberName) && typeName.TypeParamCount == typeArgs.Length)
                {
                    return MemberTypeInstantiator.Instantiate(factory, this, memberTypeDecl, typeArgs);
                }
            }

            return null;
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public M.NormalTypeId MakeChildTypeId(M.Name name, ImmutableArray<M.TypeId> typeArgs)
        {
            var thisTypeId = this.GetMTypeId();
            return new M.MemberTypeId(thisTypeId, name, typeArgs);
        }

        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            return typeArgs;
        }

        // for covariance
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeSymbolNode ITypeSymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        R.Path.Normal ISymbolNode.MakeRPath() => MakeRPath();
        ITypeDeclSymbolNode ITypeSymbolNode.GetDeclSymbolNode() => decl;
    }
}
