using Gum.Infra;
using System.Collections.Generic;
using Gum.Collections;

using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;

namespace Gum.IR0Translator
{
    [ImplementIEquatable]
    partial class StructTypeValue : NormalTypeValue
    {
        ItemValueFactory itemValueFactory;
        RItemFactory ritemFactory;

        ItemValueOuter outer;

        IModuleStructInfo structInfo;
        ImmutableArray<TypeValue> typeArgs;

        internal StructTypeValue(ItemValueFactory factory, RItemFactory ritemFactory, ItemValueOuter outer, IModuleStructInfo structInfo, ImmutableArray<TypeValue> typeArgs)
        {
            this.itemValueFactory = factory;
            this.ritemFactory = ritemFactory;
            this.outer = outer;
            this.structInfo = structInfo;
            this.typeArgs = typeArgs;
        }

        ItemQueryResult GetMember_Type(M.Name memberName, int typeParamCount)
        {
            var candidates = new Candidates<ItemQueryResult.Valid>();

            foreach (var memberType in structInfo.GetMemberTypes())
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
            foreach (var memberFunc in structInfo.GetMemberFuncs())
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

            foreach (var memberVar in structInfo.GetMemberVars())
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

        public StructTypeValue? GetBaseType()
        {
            var baseType = structInfo.GetBaseStruct();
            if (baseType == null) return null;

            var typeEnv = MakeTypeEnv();
            return baseType.Apply_StructTypeValue(typeEnv);
        }

        public override ItemQueryResult GetMember(M.Name memberName, int typeParamCount)
        {
            if (memberName.Equals(M.SpecialNames.Constructor))
                return new ItemQueryResult.Constructors(this, structInfo.GetConstructors());

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
                return ItemQueryResult.NotFound.Instance;

            return results[0];
        }

        public override TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs)
        {
            // TODO: caching
            foreach (var memberType in structInfo.GetMemberTypes())
            {
                if (memberType.GetName().Equals(memberName) && memberType.GetTypeParams().Length == typeArgs.Length)
                    return itemValueFactory.MakeTypeValue(this, memberType, typeArgs);
            }

            return null;
        }

        internal override void FillTypeEnv(TypeEnvBuilder builder) 
        {
            if (outer != null)
                outer.FillTypeEnv(builder);            

            for (int i = 0; i < structInfo.GetTypeParams().Length; i++)
                builder.Add(typeArgs[i]);
        }

        public StructTypeValue Apply_StructTypeValue(TypeEnv typeEnv)
        {
            // [X<00T>.Y<10U>.Z<20V>].Apply([00 => int, 10 => short, 20 => string])

            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply_TypeValue(typeEnv));
            return itemValueFactory.MakeStructValue(appliedOuter, structInfo, appliedTypeArgs);
        }

        public sealed override NormalTypeValue Apply_NormalTypeValue(TypeEnv typeEnv)
        {
            return Apply_StructTypeValue(typeEnv);
        }
        
        public override R.Path.Nested GetRPath_Nested()
        {
            var rname = RItemFactory.MakeName(structInfo.GetName());
            var rtypeArgs = RItemFactory.MakeRTypes(typeArgs);

            return new R.Path.Nested(outer.GetRPath(), rname, new R.ParamHash(structInfo.GetTypeParams().Length, default), rtypeArgs);
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
        {
            return new R.StructMemberLoc(instance, member);
        }

        public ConstructorValue? GetTrivialConstructor()
        {
            var constructorInfo = structInfo.GetTrivialConstructor();
            if (constructorInfo == null) return null;

            return itemValueFactory.MakeConstructorValue(this, constructorInfo);
        }

        public override int GetTotalTypeParamCount()
        {
            return outer.GetTotalTypeParamCount() + structInfo.GetTypeParams().Length;
        }
    }
}
