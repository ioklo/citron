using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Gum.IR0
{
    class TypeValueApplier
    {
        ItemInfoRepository itemInfoRepo;

        public TypeValueApplier(ItemInfoRepository itemInfoRepo)
        {
            this.itemInfoRepo = itemInfoRepo;
        }

        void FillTypeEnv(int depth, ImmutableArray<string> typeParams, ImmutableArray<TypeValue> typeArgs, Dictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {   
            Debug.Assert(typeParams.Length == typeArgs.Length);

            for (int i = 0; i < typeParams.Length ; i++)
                typeEnv[new TypeValue.TypeVar(depth, i, typeParams[i])] = typeArgs[i];
        }

        // class X<T> { class Y<T> { } } 일때
        // FillTypeEnv(X<int>.Y<short>, env) -> env with {Tx -> int, Ty -> short}
        (int Depth, TypeInfo ParentTypeInfo) FillTypeEnv(TypeValue.Normal ntv, Dictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {
            // base case 
            // [Module]Namespace.X
            var outer = ntv.GetOuter();
            if (outer == null)
            {
                var typeInfo = itemInfoRepo.GetItem<TypeInfo>(ntv.GetTypeId());
                Debug.Assert(typeInfo != null);

                // update env
                FillTypeEnv(0, typeInfo.GetTypeParams().ToImmutableArray(), ntv.Entry.TypeArgs, typeEnv);
                return (1, typeInfo);
            }
            else
            {
                // [Module]Namespace.X<int>.Y<short>
                // var parentTypeInfo = FillTypeEnv([Module]Namespace.X<int>, env)
                // var typeInfo = typeInfo.GetTypeInfo(Y);
                // update env
                // return typeInfo;

                var (depth, parentTypeInfo) = FillTypeEnv(outer, typeEnv);
                var typeInfo = parentTypeInfo.GetItem(ntv.Entry.GetItemPathEntry()) as TypeInfo;
                Debug.Assert(typeInfo != null);

                FillTypeEnv(depth, typeInfo.GetTypeParams().ToImmutableArray(), ntv.Entry.TypeArgs, typeEnv);
                return (depth + 1, typeInfo);
            }
        }      
        


        // ApplyTypeEnv_Normal(Normal (Z, [[T], [U], []]), { T -> int, U -> short })
        // 
        // Normal(Z, [[int], [short], []])
        private TypeValue ApplyTypeEnv_Normal(TypeValue.Normal ntv, Dictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {
            var appliedOuterEntries = ntv.OuterEntries.Select(outerEntry =>
                new AppliedItemPathEntry(outerEntry.Name, outerEntry.ParamHash, outerEntry.TypeArgs.Select(typeArg => ApplyTypeEnv(typeArg, typeEnv)))
            );

            var appliedEntry = new AppliedItemPathEntry(ntv.Entry.Name, ntv.Entry.ParamHash, ntv.Entry.TypeArgs.Select(typeArg => ApplyTypeEnv(typeArg, typeEnv)));

            return new TypeValue.Normal(ntv.ModuleName, ntv.NamespacePath, appliedOuterEntries, appliedEntry);
        }

        // 
        private TypeValue.Func ApplyTypeEnv_Func(TypeValue.Func typeValue, Dictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {
            return new TypeValue.Func(
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

        // class X<T> { class Y<U> { S<T>.List<U> u; } } => ApplyTypeValue_Normal(X<int>.Y<short>, S<T>.List<U>) => S<int>.Dict<short>
        private TypeValue Apply_Normal(TypeValue.Normal context, TypeValue typeValue)
        {
            var typeEnv = new Dictionary<TypeValue.TypeVar, TypeValue>(ModuleInfoEqualityComparer.Instance);

            FillTypeEnv(context, typeEnv);

            return ApplyTypeEnv(typeValue, typeEnv);
        }

        // 주어진 funcValue 컨텍스트 내에서, typeValue를 치환하기
        public TypeValue.Func Apply_Func(FuncValue context, TypeValue.Func typeValue)
        {
            var typeEnv = new Dictionary<TypeValue.TypeVar, TypeValue>(ModuleInfoEqualityComparer.Instance);            

            var outer = context.GetOuter();
            if (outer == null)
            {
                var funcInfo = itemInfoRepo.GetItem<FuncInfo>(context.GetFuncId());
                Debug.Assert(funcInfo != null);

                FillTypeEnv(0, funcInfo.TypeParams, context.Entry.TypeArgs, typeEnv);
            }
            else
            {
                var (depth, typeInfo) = FillTypeEnv(outer, typeEnv);
                Debug.Assert(typeInfo != null);

                var funcInfo = typeInfo.GetItem(context.Entry.GetItemPathEntry()) as FuncInfo;
                Debug.Assert(funcInfo != null);

                FillTypeEnv(depth, funcInfo.TypeParams, context.Entry.TypeArgs, typeEnv);
            }

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