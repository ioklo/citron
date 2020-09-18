using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Gum
{
    public class TypeValueApplier
    {
        ModuleInfoService moduleInfoService;

        public TypeValueApplier(ModuleInfoService moduleInfoService)
        {
            this.moduleInfoService = moduleInfoService;
        }

        private void MakeTypeEnv_Type(ModuleItemId typeId, TypeArgumentList typeArgList, Dictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {
            var typeInfo = moduleInfoService.GetTypeInfos(typeId).SingleOrDefault();
            if (typeInfo == null)
                Debug.Assert(false);

            if (typeInfo.OuterTypeId != null)
                MakeTypeEnv_Type(typeInfo.OuterTypeId, typeArgList.Outer!, typeEnv);

            var typeParams = typeInfo.GetTypeParams();

            Debug.Assert(typeParams.Count == typeArgList.Args.Length);

            for (int i = 0; i < typeParams.Count; i++)
                typeEnv[TypeValue.MakeTypeVar(typeId, typeParams[i])] = typeArgList.Args[i];
        }

        // ApplyTypeEnv_Normal(Normal (Z, [[T], [U], []]), { T -> int, U -> short })
        // 
        // Normal(Z, [[int], [short], []])

        private TypeArgumentList ApplyTypeEnv_TypeArgumentList(TypeArgumentList typeArgList, Dictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {
            TypeArgumentList? appliedOuter = null;

            if (typeArgList.Outer != null)
                appliedOuter = ApplyTypeEnv_TypeArgumentList(typeArgList.Outer, typeEnv);

            var appliedTypeArgs = new List<TypeValue>(typeArgList.Args.Length);
            foreach (var typeArg in typeArgList.Args)
            {
                var appliedTypeArg = ApplyTypeEnv(typeArg, typeEnv);
                appliedTypeArgs.Add(appliedTypeArg);
            }

            return TypeArgumentList.Make(appliedOuter, appliedTypeArgs);
        }

        private TypeValue ApplyTypeEnv_Normal(TypeValue.Normal ntv, Dictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {
            var appliedTypeArgList = ApplyTypeEnv_TypeArgumentList(ntv.TypeArgList, typeEnv);
            return TypeValue.MakeNormal(ntv.TypeId, appliedTypeArgList);
        }

        // 
        private TypeValue.Func ApplyTypeEnv_Func(TypeValue.Func typeValue, Dictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {
            return TypeValue.MakeFunc(
                ApplyTypeEnv(typeValue.Return, typeEnv),
                typeValue.Params.Select(parameter => ApplyTypeEnv(parameter, typeEnv)));
        }

        // T, [T -> ]
        private TypeValue ApplyTypeEnv_TypeVar(TypeValue.TypeVar typeValue, Dictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {
            if (typeEnv.TryGetValue(typeValue, out var appliedTypeValue))
                return appliedTypeValue;

            return typeValue;
        }

        private TypeValue ApplyTypeEnv(TypeValue typeValue, Dictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {
            return typeValue switch
            {
                TypeValue.Normal normalTypeValue => ApplyTypeEnv_Normal(normalTypeValue, typeEnv),
                TypeValue.Func funcTypeValue => ApplyTypeEnv_Func(funcTypeValue, typeEnv),
                TypeValue.TypeVar typeVarTypeValue => ApplyTypeEnv_TypeVar(typeVarTypeValue, typeEnv),
                TypeValue.Void vtv => vtv,
                _ => throw new NotImplementedException()
            };
        }

        // class X<T> { class Y<U> { S<T>.List<U> u; } } => ApplyTypeValue_Normal(X<int>.Y<short>, S<T>.List<U>, context) => S<int>.Dict<short>
        private TypeValue Apply_Normal(TypeValue.Normal context, TypeValue typeValue)
        {
            var typeEnv = new Dictionary<TypeValue.TypeVar, TypeValue>();

            MakeTypeEnv_Type(context.TypeId, context.TypeArgList, typeEnv);

            return ApplyTypeEnv(typeValue, typeEnv);
        }

        // 주어진 funcValue 컨텍스트 내에서, typeValue를 치환하기
        public TypeValue.Func Apply_Func(FuncValue context, TypeValue.Func typeValue)
        {
            var funcInfo = moduleInfoService.GetFuncInfos(context.FuncId).Single();

            var typeEnv = new Dictionary<TypeValue.TypeVar, TypeValue>();
            if (funcInfo.OuterId != null)
            {
                // TODO: Outer가 꼭 TypeId이지는 않을 것 같다. FuncId일 수도
                MakeTypeEnv_Type(funcInfo.OuterId, context.TypeArgList.Outer!, typeEnv);
            }

            for (int i = 0; i < funcInfo.TypeParams.Length; i++)
                typeEnv[TypeValue.MakeTypeVar(funcInfo.FuncId, funcInfo.TypeParams[i])] = context.TypeArgList.Args[i];

            return ApplyTypeEnv_Func(typeValue, typeEnv);
        }

        public TypeValue Apply(TypeValue? context, TypeValue typeValue)
        {
            if (context is TypeValue.Normal context_normal)
                return Apply_Normal(context_normal, typeValue);

            return typeValue;
        }
    }
}