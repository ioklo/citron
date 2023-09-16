using System;
using System.Collections.Generic;
using System.Diagnostics;

using Citron.Collections;
using Citron.Infra;

using Pretune;

using static Citron.Symbol.SymbolMisc;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class ClassSymbol : ITypeSymbol, ICyclicEqualityComparableClass<ClassSymbol>
    {
        SymbolFactory factory;
        ISymbolNode outer;
        ClassDeclSymbol decl;
        ImmutableArray<IType> typeArgs;

        TypeEnv typeEnv;

        internal ClassSymbol(SymbolFactory factory, ISymbolNode outer, ClassDeclSymbol decl, ImmutableArray<IType> typeArgs)
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
            var baseClass = decl.GetBaseClass();
            if (baseClass == null) return null;

            // ClassSymbol가 X<int>.C<TV(4)>라면 TV(0)을 int로, TV(1)을 TV(4)로 치환한다
            // TV(4)는 x<int>.C<TV(4)>를 선언한 환경이다
            return baseClass.Apply(typeEnv);
        }

        // except itself
        public bool IsBaseOf(ClassSymbol derivedClass)
        {
            ClassSymbol? curBaseType = derivedClass.GetBaseClass();

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
        SymbolQueryResult? QueryMember_Type(Name memberName, int typeParamCount)
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

            return candidates.MakeSymbolQueryResult();
        }

        SymbolQueryResult? QueryMember_Funcs(Name memberName, int typeParamCount)
        {
            var builder = ImmutableArray.CreateBuilder<DeclAndConstructor<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>>();

            foreach (var memberFunc in decl.GetMemberFuncs())
            {
                var funcName = memberFunc.GetNodeName();

                if (funcName.Name.Equals(memberName) &&
                    typeParamCount <= funcName.TypeParamCount)
                {
                    var dac = new DeclAndConstructor<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>(
                        memberFunc,
                        typeArgs => factory.MakeClassMemberFunc(this, memberFunc, typeArgs)
                    );

                    builder.Add(dac);
                }
            }

            // 여러개 있을 수 있기때문에 MultipleCandidates를 리턴하지 않는다
            if (builder.Count == 0)
                return null;

            return new SymbolQueryResult.ClassMemberFuncs(builder.ToImmutable());
        }

        SymbolQueryResult? QueryMember_Var(Name memberName)
        {
            var candidates = new Candidates<SymbolQueryResult>();

            int count = decl.GetMemberVarCount();            
            for (int i = 0; i < count; i++)
            {
                var memberVar = decl.GetMemberVar(i);
                if (memberVar.GetName().Equals(memberName))
                {
                    candidates.Add(new SymbolQueryResult.ClassMemberVar(factory.MakeClassMemberVar(this, memberVar)));
                }
            }

            return candidates.MakeSymbolQueryResult();
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

        SymbolQueryResult? ISymbolNode.QueryMember(Name memberName, int explicitTypeArgsCount)
            => QueryMember(memberName, explicitTypeArgsCount);

        public SymbolQueryResult? QueryMember(Name memberName, int explicitTypeArgsCount)
        {   
            // TODO: caching
            var results = ImmutableArray.CreateBuilder<SymbolQueryResult>();

            // error, notfound, found
            var typeResult = QueryMember_Type(memberName, explicitTypeArgsCount);            
            if (typeResult is SymbolQueryResult.MultipleCandidatesError) return typeResult;
            if (typeResult != null) results.Add(typeResult);

            // error, notfound, found
            var funcResult = QueryMember_Funcs(memberName, explicitTypeArgsCount);
            if (funcResult is SymbolQueryResult.MultipleCandidatesError) return funcResult;
            if (funcResult != null) results.Add(funcResult);

            // error, notfound, found
            if (explicitTypeArgsCount == 0)
            {
                var varResult = QueryMember_Var(memberName);
                if (varResult is SymbolQueryResult.MultipleCandidatesError) return varResult;
                if (varResult != null) results.Add(varResult);
            }

            if (1 < results.Count)
                return new SymbolQueryResult.MultipleCandidatesError(results.ToImmutable());

            if (results.Count == 0)
            {
                var baseClass = GetBaseClass();
                if (baseClass == null)
                    return null;

                return baseClass.QueryMember(memberName, explicitTypeArgsCount);
            }
            else
            {
                return results[0];
            }
        }

        public IType? GetMemberType(Name memberName, ImmutableArray<IType> typeArgs)
        {
            // TODO: caching
            foreach (var memberTypeDecl in decl.GetMemberTypes())
            {
                var typeName = memberTypeDecl.GetNodeName();

                if (typeName.Name.Equals(memberName) && typeName.TypeParamCount == typeArgs.Length)
                {
                    return SymbolInstantiator.Instantiate(factory, this, memberTypeDecl, typeArgs).MakeType();
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

        public IType GetTypeArg(int index)
        {
            return typeArgs[index];
        }

        // for covariance
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);        
        ITypeDeclSymbol ITypeSymbol.GetDeclSymbolNode() => decl;

        IType ITypeSymbol.MakeType()
        {
            return new ClassType(this);
        }        

        void ISymbolNode.Accept<TVisitor>(ref TVisitor visitor)
        {
            visitor.VisitClass(this);
        }

        void ITypeSymbol.Accept<TTypeSymbolVisitor>(ref TTypeSymbolVisitor visitor)
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

        bool ICyclicEqualityComparableClass<ITypeSymbol>.CyclicEquals(ITypeSymbol other, ref CyclicEqualityCompareContext context)
            => other is ClassSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ClassSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ClassSymbol>.CyclicEquals(ClassSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(ClassSymbol other, ref CyclicEqualityCompareContext context)
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
    }
}
