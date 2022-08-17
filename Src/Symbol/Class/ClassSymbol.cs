using System;
using System.Collections.Generic;

using Citron.Collections;
using Citron.Infra;

using Pretune;

using Citron.Module;
using System.Diagnostics;

namespace Citron.Symbol
{
    // value
    [ImplementIEquatable]
    public partial class ClassSymbol : ITypeSymbol
    {
        SymbolFactory factory;
        ISymbolNode outer;
        ClassDeclSymbol decl;
        ImmutableArray<ITypeSymbol> typeArgs;

        TypeEnv typeEnv;

        internal ClassSymbol(SymbolFactory factory, ISymbolNode outer, ClassDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
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
        public ClassSymbol? GetBaseClass() 
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
            ClassSymbol? curBaseType = derivedType.GetBaseClass();

            while(curBaseType != null)
            {
                if (Equals(curBaseType)) return true;
                curBaseType = curBaseType.GetBaseClass();
            }

            return false;
        }
        
        public ClassSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeClass(appliedOuter, decl, appliedTypeArgs);
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
        SymbolQueryResult QueryMember_Type(Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<SymbolQueryResult>();
            var nodeName = new DeclSymbolNodeName(memberName, typeParamCount, default);

            foreach (var memberTypeDecl in decl.GetMemberTypes())
            {
                // 이름이 같고, 타입 파라미터 개수가 같다면
                if (nodeName.Equals(memberTypeDecl.GetNodeName()))
                {
                    var symbolQueryResult = SymbolQueryResultBuilder.Build(memberTypeDecl, this, factory);
                    candidates.Add(symbolQueryResult);
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

        SymbolQueryResult QueryMember_Func(Name memberName, int typeParamCount)
        {
            var builder = ImmutableArray.CreateBuilder<DeclAndConstructor<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>>();

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

                    var dac = new DeclAndConstructor<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>(
                        memberFunc,
                        typeArgs => factory.MakeClassMemberFunc(this, memberFunc, typeArgs)
                    );

                    builder.Add(dac);
                }
            }

            // 여러개 있을 수 있기때문에 MultipleCandidates를 리턴하지 않는다
            if (builder.Count == 0)
                return SymbolQueryResult.NotFound.Instance;

            // 둘다 가지고 있으면 안된다
            if (bHaveInstance && bHaveStatic)
                return SymbolQueryResult.Error.MultipleCandidates.Instance;

            return new SymbolQueryResult.ClassMemberFuncs(builder.ToImmutable());
        }

        SymbolQueryResult QueryMember_Var(Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<ClassMemberVarSymbol>();

            int count = decl.GetMemberVarCount();            
            for (int i = 0; i < count; i++)
            {
                var memberVar = decl.GetMemberVar(i);
                if (memberVar.GetName().Equals(memberName))
                {
                    candidates.Add(factory.MakeClassMemberVar(this, memberVar));
                }
            }

            var result = candidates.GetSingle();
            if (result != null)
            {
                // 변수를 찾았는데 타입 아규먼트가 있다면 에러
                if (typeParamCount != 0)
                    return SymbolQueryResult.Error.VarWithTypeArg.Instance;

                return new SymbolQueryResult.ClassMemberVar(result);
            }

            if (candidates.HasMultiple)
                return SymbolQueryResult.Error.MultipleCandidates.Instance;

            if (candidates.IsEmpty)
                return SymbolQueryResult.NotFound.Instance;

            throw new UnreachableCodeException();
        }

        public ClassConstructorSymbol GetConstructor(int index)
        {
            var constructorDecl = decl.GetConstructor(index);
            return factory.MakeClassConstructor(this, constructorDecl);
        }

        public int GetConstructorCount()
        {
            return decl.GetConstructorCount();
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
                return SymbolQueryResult.Error.MultipleCandidates.Instance;

            if (results.Count == 0)
            {
                var baseTypeValue = GetBaseClass();
                if (baseTypeValue == null)
                    return SymbolQueryResult.NotFound.Instance;

                return baseTypeValue.QueryMember(memberName, typeParamCount);
            }
            else
            {
                return results[0];
            }
        }

        public ITypeSymbol? GetMemberType(Name memberName, ImmutableArray<ITypeSymbol> typeArgs)
        {
            // TODO: caching
            foreach (var memberTypeDecl in decl.GetMemberTypes())
            {
                var typeName = memberTypeDecl.GetNodeName();

                if (typeName.Name.Equals(memberName) && typeName.TypeParamCount == typeArgs.Length)
                {
                    return SymbolInstantiator.Instantiate(factory, this, memberTypeDecl, typeArgs);
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

        public ITypeSymbol GetTypeArg(int index)
        {
            return typeArgs[index];
        }

        // for covariance
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);        
        ITypeDeclSymbol ITypeSymbol.GetDeclSymbolNode() => decl;

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitClass(this);
        }

        public ClassMemberVarSymbol? GetMemberVar(Name name)
        {
            var memberVarDecl = decl.GetMemberVar(name);
            if (memberVarDecl == null) return null;

            return factory.MakeClassMemberVar(this, memberVarDecl);
        }

        public int GetMemberVarCount()
        {
            return decl.GetMemberVarCount();
        }

        public ClassMemberVarSymbol GetMemberVar(int index)
        {
            var memberVar = decl.GetMemberVar(index);
            return factory.MakeClassMemberVar(this, memberVar);
        }

        public ClassDeclSymbol GetDecl()
        {
            return decl;
        }        
    }
}
