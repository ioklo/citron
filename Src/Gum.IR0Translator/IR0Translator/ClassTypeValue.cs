using System;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;
using Gum.Collections;
using Gum.Infra;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    // closed type
    [AutoConstructor, ImplementIEquatable]
    partial class ClassTypeValue : NormalTypeValue
    {
        ItemValueFactory itemValueFactory;

        ItemValueOuter outer;
        IModuleClassInfo classInfo;            // C
        ImmutableArray<TypeValue> typeArgs;    // <T0, T1>

        // class X<T> { class Y<U> : Z<T>.B<T> }
        // X<int>.Y<bool> c; // c의 classTypeValue { [int, bool], X<>.Y<> }
        // baseof(c) //  classTypeValue { [int, int], Z<>.B<> } <- 여기에
        
        public ClassTypeValue? GetBaseType() 
        {
            // 지금 속한 클래스의 타입 환경에 종속된 ClassTypeValue를 돌려준다
            // X<T>.C<U> : B<U, T> => ClassTypeValue(B, [TV(1), TV(0)])
            var baseClassTypeValue = classInfo.GetBaseClass();
            if (baseClassTypeValue == null) return null;

            // ClassTypeValue가 X<int>.C<TV(4)>라면 TV(0)을 int로, TV(1)을 TV(4)로 치환한다
            // TV(4)는 x<int>.C<TV(4)>를 선언한 환경이다
            var typeEnv = MakeTypeEnv(); // 현재 클래스의 TypeEnv
            return baseClassTypeValue.Apply_ClassTypeValue(typeEnv);
        }

        // except itself
        public bool IsBaseOf(ClassTypeValue derivedType)
        {
            ClassTypeValue? curBaseType = derivedType.GetBaseType();

            while(curBaseType != null)
            {
                if (Equals(curBaseType)) return true;
                curBaseType = curBaseType.GetBaseType();
            }

            return false;
        }

        public ClassTypeValue Apply_ClassTypeValue(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply_TypeValue(typeEnv));
            return itemValueFactory.MakeClassValue(appliedOuter, classInfo, appliedTypeArgs);
        }

        public sealed override NormalTypeValue Apply_NormalTypeValue(TypeEnv typeEnv)
        {
            return Apply_ClassTypeValue(typeEnv);
        }

        public override R.Path.Nested GetRPath_Nested()
        {
            var rname = RItemFactory.MakeName(classInfo.GetName());
            var rtypeArgs = RItemFactory.MakeRTypes(typeArgs);

            return new R.Path.Nested(outer.GetRPath(), rname, new R.ParamHash(classInfo.GetTypeParams().Length, default), rtypeArgs);
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested memberPath)
        {
            return new R.ClassMemberLoc(instance, memberPath);
        }

        public override int GetTotalTypeParamCount()
        {
            return outer.GetTotalTypeParamCount() + classInfo.GetTypeParams().Length;
        }

        internal override void FillTypeEnv(TypeEnvBuilder builder)
        {
            if (outer != null)
                outer.FillTypeEnv(builder);

            for (int i = 0; i < classInfo.GetTypeParams().Length; i++)
                builder.Add(typeArgs[i]);
        }

        // 인자와 멤버변수가 1:1로 매칭되는 constructor를 의미한다. (base 클래스 포함). generate해야 한다. 미리 선언한 constructor는 trivial이 아니다
        public ConstructorValue? GetTrivialConstructor()
        {
            var constructorInfo = classInfo.GetTrivialConstructor();
            if (constructorInfo == null) return null;

            return itemValueFactory.MakeConstructorValue(this, constructorInfo);
        }

        // 빈 constructor를 말한다 (Access 체크는 하지 않는다)
        public ConstructorValue? GetDefaultConstructor()
        {
            foreach (var constructorInfo in classInfo.GetConstructors())
            {
                var paramTypes = constructorInfo.GetParamTypes();

                if (paramTypes.Length == 0)
                    return itemValueFactory.MakeConstructorValue(this, constructorInfo);
            }

            return null;
        }

        ItemQueryResult GetMember_Type(M.Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<ItemQueryResult.Valid>();

            foreach (var memberType in classInfo.GetMemberTypes())
            {
                // 이름이 같고, 타입 파라미터 개수가 같다면
                if (memberType.GetName().Equals(memberName) && memberType.GetTypeParams().Length == typeParamCount)
                {
                    candidates.Add(new ItemQueryResult.Type(new NestedItemValueOuter(this), memberType));
                }
            }

            var result = candidates.GetSingle();
            if (result != null)
                return result;

            if (candidates.HasMultiple)
                return ItemQueryResult.Error.MultipleCandidates.Instance;

            if (candidates.IsEmpty)
                return ItemQueryResult.NotFound.Instance;

            throw new UnreachableCodeException();
        }

        ItemQueryResult GetMember_Func(M.Name memberName, int typeParamCount)
        {
            var funcsBuilder = ImmutableArray.CreateBuilder<IModuleFuncInfo>();

            bool bHaveInstance = false;
            bool bHaveStatic = false;
            foreach (var memberFunc in classInfo.GetMemberFuncs())
            {
                if (memberFunc.GetName().Equals(memberName) &&
                    typeParamCount <= memberFunc.GetTypeParams().Length)
                {
                    bHaveInstance |= memberFunc.IsInstanceFunc();
                    bHaveStatic |= !memberFunc.IsInstanceFunc();

                    funcsBuilder.Add(memberFunc);
                }
            }

            // 여러개 있을 수 있기때문에 MultipleCandidates를 리턴하지 않는다
            if (funcsBuilder.Count == 0)
                return ItemQueryResult.NotFound.Instance;

            // 둘다 가지고 있으면 안된다
            if (bHaveInstance && bHaveStatic)
                return ItemQueryResult.Error.MultipleCandidates.Instance;

            return new ItemQueryResult.Funcs(new NestedItemValueOuter(this), funcsBuilder.ToImmutable(), bHaveInstance);
        }

        ItemQueryResult GetMember_Var(M.Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<ItemQueryResult.MemberVar>();

            foreach (var memberVar in classInfo.GetMemberVars())
                if (memberVar.GetName().Equals(memberName))
                {
                    candidates.Add(new ItemQueryResult.MemberVar(this, memberVar));
                }

            var result = candidates.GetSingle();
            if (result != null)
            {
                // 변수를 찾았는데 타입 아규먼트가 있다면 에러
                if (typeParamCount != 0)
                    return ItemQueryResult.Error.VarWithTypeArg.Instance;

                return result;
            }

            if (candidates.HasMultiple)
                return ItemQueryResult.Error.MultipleCandidates.Instance;

            if (candidates.IsEmpty)
                return ItemQueryResult.NotFound.Instance;

            throw new UnreachableCodeException();
        }

        public override ItemQueryResult GetMember(M.Name memberName, int typeParamCount) 
        {
            if (memberName.Equals(M.Name.Constructor))
                return new ItemQueryResult.Constructors(this, classInfo.GetConstructors());

            // TODO: caching
            var results = new List<ItemQueryResult.Valid>();

            // error, notfound, found
            var typeResult = GetMember_Type(memberName, typeParamCount);
            if (typeResult is ItemQueryResult.Error) return typeResult;
            if (typeResult is ItemQueryResult.Valid typeMemberResult) results.Add(typeMemberResult);

            // error, notfound, found
            var funcResult = GetMember_Func(memberName, typeParamCount);
            if (funcResult is ItemQueryResult.Error) return funcResult;
            if (funcResult is ItemQueryResult.Valid funcMemberResult) results.Add(funcMemberResult);

            // error, notfound, found
            var varResult = GetMember_Var(memberName, typeParamCount);
            if (varResult is ItemQueryResult.Error) return varResult;
            if (varResult is ItemQueryResult.Valid varMemberResult) results.Add(varMemberResult);

            if (1 < results.Count)
                return ItemQueryResult.Error.MultipleCandidates.Instance;

            if (results.Count == 0)
            {
                var baseTypeValue = GetBaseType();
                if (baseTypeValue == null)
                    return ItemQueryResult.NotFound.Instance;

                return baseTypeValue.GetMember(memberName, typeParamCount);
            }
            else
            {
                return results[0];
            }
        }

        public override TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs) 
        {
            // TODO: caching
            foreach (var memberType in classInfo.GetMemberTypes())
            {
                if (memberType.GetName().Equals(memberName) && memberType.GetTypeParams().Length == typeArgs.Length)
                    return itemValueFactory.MakeTypeValue(this, memberType, typeArgs);
            }

            return null;
        }
    }
}
