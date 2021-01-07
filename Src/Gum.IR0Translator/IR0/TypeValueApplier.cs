using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Gum.IR0
{
    public class TypeValueApplier
    {
        ModuleInfoRepository moduleInfoRepo;
        
        public TypeValueApplier(ModuleInfoRepository moduleInfoRepo)
        {
            this.moduleInfoRepo = moduleInfoRepo;
        }

        void FillTypeEnv(int depth, IReadOnlyList<string> typeParams, ImmutableArray<TypeValue> typeArgs, Dictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {   
            Debug.Assert(typeParams.Count == typeArgs.Length);

            for (int i = 0; i < typeParams.Count; i++)
                typeEnv[new TypeValue.TypeVar(depth, i, typeParams[i])] = typeArgs[i];
        }

        // MakeTypeEnv_Type(X<T>, [int]) -> [T -> int]
        private void MakeTypeEnv_Type(TypeValue.Normal ntv, Dictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {
            // IModuleInfo, NamespacePath, OuterEntries, Entry
            var moduleInfo = moduleInfoRepo.GetModule(ntv.ModuleName);
            Debug.Assert(moduleInfo != null);

            if (ntv.OuterEntries.Length == 0)
            {
                var typeInfo = moduleInfo.GetItem(ntv.NamespacePath, ntv.Entry.GetItemPathEntry()) as TypeInfo;
                Debug.Assert(typeInfo != null);

                FillTypeEnv(0, typeInfo.GetTypeParams(), ntv.Entry.TypeArgs, typeEnv);
            }
            else
            {
                var typeInfo = moduleInfo.GetItem(ntv.NamespacePath, ntv.OuterEntries[0].GetItemPathEntry()) as TypeInfo;
                Debug.Assert(typeInfo != null);

                FillTypeEnv(0, typeInfo.GetTypeParams(), ntv.OuterEntries[0].TypeArgs, typeEnv);

                for (int i = 1; i < ntv.OuterEntries.Length; i++)
                {                    
                    typeInfo = typeInfo.GetItem(ntv.OuterEntries[i].GetItemPathEntry()) as TypeInfo;
                    Debug.Assert(typeInfo != null);

                    FillTypeEnv(i, typeInfo.GetTypeParams(), ntv.OuterEntries[i].TypeArgs, typeEnv);
                }

                typeInfo = typeInfo.GetItem(ntv.Entry.GetItemPathEntry()) as TypeInfo;
                Debug.Assert(typeInfo != null);

                FillTypeEnv(ntv.OuterEntries.Length, typeInfo.GetTypeParams(), ntv.Entry.TypeArgs, typeEnv);
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

            MakeTypeEnv_Type(context, typeEnv);

            return ApplyTypeEnv(typeValue, typeEnv);
        }

        // 주어진 funcValue 컨텍스트 내에서, typeValue를 치환하기
        public TypeValue.Func Apply_Func(FuncValue context, TypeValue.Func typeValue)
        {
            var typeEnv = new Dictionary<TypeValue.TypeVar, TypeValue>(ModuleInfoEqualityComparer.Instance);

            var moduleInfo = moduleInfoRepo.GetModule(context.ModuleName);
            Debug.Assert(moduleInfo != null);

            if (context.OuterEntries.Length == 0)
            {
                var funcInfo = moduleInfo.GetItem(context.NamespacePath, context.Entry.GetItemPathEntry()) as FuncInfo;
                Debug.Assert(funcInfo != null);

                FillTypeEnv(0, funcInfo.TypeParams, context.Entry.TypeArgs, typeEnv);
            }
            else
            {
                var typeInfo = moduleInfo.GetItem(context.NamespacePath, context.OuterEntries[0].GetItemPathEntry()) as TypeInfo;
                Debug.Assert(typeInfo != null);
                FillTypeEnv(0, typeInfo.GetTypeParams(), context.OuterEntries[0].TypeArgs, typeEnv);

                for (int i = 1; i < context.OuterEntries.Length; i++)
                {
                    typeInfo = typeInfo.GetItem(context.OuterEntries[i].GetItemPathEntry()) as TypeInfo;
                    Debug.Assert(typeInfo != null);

                    FillTypeEnv(i, typeInfo.GetTypeParams(), context.OuterEntries[i].TypeArgs, typeEnv);
                }

                var funcInfo = typeInfo.GetItem(context.Entry.GetItemPathEntry()) as FuncInfo;
                Debug.Assert(funcInfo != null);

                FillTypeEnv(context.OuterEntries.Length, funcInfo.TypeParams, context.Entry.TypeArgs, typeEnv);
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