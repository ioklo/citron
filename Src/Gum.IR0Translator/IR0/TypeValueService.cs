using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;
using Gum.CompileTime;
using System.Diagnostics;

namespace Gum.IR0
{
    class TypeValueService
    {
        ItemInfoRepository itemInfoRepo;
        TypeValueApplier typeValueApplier;

        public TypeValueService(ItemInfoRepository itemInfoRepo, TypeValueApplier typeValueApplier)
        {
            this.itemInfoRepo = itemInfoRepo;
            this.typeValueApplier = typeValueApplier;
        }

        //private bool GetMemberVarInfo(QsMetaItemId typeId, QsName name, [NotNullWhen(true)] out QsVarInfo? outVarInfo)
        //{
        //    outVarInfo = null;

        //    var typeInfo = metadataService.GetTypesById(typeId).SingleOrDefault();
        //    if (typeInfo == null)
        //        return false;

        //    if (!typeInfo.GetMemberVarId(name, out var varId))
        //        return false;

        //    outVarInfo = metadataService.GetVarsById(varId.Value).SingleOrDefault();
        //    return outVarInfo != null;
        //}

        //public bool GetMemberVarTypeValue(QsTypeValue typeValue, QsName name, [NotNullWhen(true)] out QsTypeValue? outTypeValue)
        //{
        //    outTypeValue = null;

        //    var typeValue_normal = typeValue as QsTypeValue.Normal;
        //    if (typeValue_normal == null) return false;

        //    if (!GetMemberVarInfo(typeValue_normal.TypeId, name, out var memberVarInfo))
        //        return false;

        //    outTypeValue = typeValueApplier.Apply(typeValue_normal, memberVarInfo.TypeValue);
        //    return true;
        //}    
        
        // class X<T> { class Y<U> { Dict<T, U> x; } } 
        // GetTypeValue(X<int>.Y<short>, x) => Dict<int, short>
        public TypeValue GetTypeValue(VarValue varValue)
        {
            var varInfo = itemInfoRepo.GetItem<VarInfo>(varValue.GetItemId());

            Debug.Assert(varInfo != null);            

            if (varValue.TypeEntries.Length != 0)
                return typeValueApplier.Apply(
                    new TypeValue.Normal(
                        varValue.ModuleName, 
                        varValue.NamespacePath, 
                        varValue.TypeEntries.SkipLast(1), 
                        varValue.TypeEntries[varValue.TypeEntries.Length - 1]), 
                    varInfo.TypeValue);
            else
                return varInfo.TypeValue;
        }

        // class X<T> { class Y<U> { S<T> F<V>(V v, List<U> u); } } => MakeFuncTypeValue(X<int>.Y<short>, F, context) 
        // (V, List<short>) => S<int>
        public TypeValue.Func GetTypeValue(FuncValue funcValue)
        {
            var funcInfo = itemInfoRepo.GetItem<FuncInfo>(funcValue.GetFuncId());
            Debug.Assert(funcInfo != null);

            // 
            TypeValue retTypeValue;
            if (funcInfo.bSeqCall)
            {
                retTypeValue = new TypeValue.Normal(ItemIds.Enumerable, new[] { new[] { funcInfo.RetTypeValue } });
            }
            else
            {
                retTypeValue = funcInfo.RetTypeValue;
            }

            return typeValueApplier.Apply_Func(funcValue, new TypeValue.Func(retTypeValue, funcInfo.ParamTypeValues));
        }


        // 
        // GetFuncTypeValue_NormalTypeValue(X<int>.Y<short>, "Func", <bool>) =>   (int, short) => bool
        // 
        //private bool GetMemberFuncTypeValue_Normal(
        //    bool bStaticOnly,
        //    QsTypeValue.Normal typeValue,
        //    QsName memberFuncId,
        //    ImmutableArray<QsTypeValue> typeArgs,
        //    [NotNullWhen(true)] out QsTypeValue.Func? funcTypeValue)
        //{
        //    funcTypeValue = null;

        //    if (!GetTypeById(typeValue.TypeId, out var type))
        //        return false;

        //    if (!type.GetMemberFuncId(memberFuncId, out var memberFunc))
        //        return false;

        //    if (!GetFuncById(memberFunc.Value.FuncId, out var func))
        //        return false;

        //    if (func.TypeParams.Length != typeArgs.Length)
        //        return false;

        //    funcTypeValue = MakeFuncTypeValue(typeValue, func, typeArgs);
        //    return true;
        //}

        //public bool GetMemberFuncTypeValue(
        //    bool bStaticOnly,
        //    QsTypeValue typeValue,
        //    QsName memberFuncId,
        //    ImmutableArray<QsTypeValue> typeArgs,
        //    [NotNullWhen(true)] out QsTypeValue.Func? funcTypeValue)
        //{
        //    // var / typeVar / normal / func

        //    if (typeValue is QsTypeValue.Normal typeValue_normal)
        //        return GetMemberFuncTypeValue_Normal(bStaticOnly, typeValue_normal, memberFuncId, typeArgs, out funcTypeValue);

        //    throw new NotImplementedException();
        //}

        // class N<T> : B<T> => N.GetBaseType => B<T(N)>
        private bool GetBaseTypeValue_Normal(TypeValue.Normal typeValue, out TypeValue? outBaseTypeValue)
        {
            outBaseTypeValue = null;

            var typeId = typeValue.GetTypeId();
            var typeInfo = itemInfoRepo.GetItem<TypeInfo>(typeId);
            if (typeInfo == null) return false;

            var baseTypeValue = typeInfo.GetBaseTypeValue();
            if (baseTypeValue == null)
                return true; // BaseType은 null일 수 있다

            outBaseTypeValue = typeValueApplier.Apply(typeValue, baseTypeValue);
            return true;
        }

        public bool GetBaseTypeValue(TypeValue typeValue, out TypeValue? baseTypeValue)
        {
            baseTypeValue = null;

            return typeValue switch
            {
                TypeValue.Normal normalTypeValue => GetBaseTypeValue_Normal(normalTypeValue, out baseTypeValue),
                _ => false
            };
        }

        public bool GetMemberFuncValue(
            TypeValue objTypeValue, 
            Name funcName, 
            IReadOnlyCollection<TypeValue> typeArgs,
            [NotNullWhen(true)] out FuncValue? outFuncValue)
        {
            outFuncValue = null;

            TypeValue.Normal? ntv = objTypeValue as TypeValue.Normal;
            if (ntv == null) return false;

            var typeInfo = itemInfoRepo.GetItem<TypeInfo>(ntv.GetTypeId());
            if (typeInfo == null)
                return false;

            var funcInfo = typeInfo.GetFuncs(funcName).SingleOrDefault();

            if (funcInfo == null)
                return false;

            // 함수는 typeArgs가 모자라도 최대한 매칭한다
            if (funcInfo.TypeParams.Length < typeArgs.Count)
                return false;

            var completedTypeArgs = typeArgs.Concat(
                funcInfo.TypeParams.Skip(typeArgs.Count).Select(typeParam =>
                   new TypeValue.TypeVar(funcInfo.GetId(), typeParam)
                )
            );

            outFuncValue = new FuncValue(
                ntv.ModuleName, 
                ntv.NamespacePath, 
                ntv.GetAllEntries(), 
                new AppliedItemPathEntry(funcInfo.GetLocalId().Name, funcInfo.GetLocalId().ParamHash, completedTypeArgs));

            return true;
        }

        public TypeValue Apply(TypeValue context, TypeValue typeValue)
        {
            return typeValueApplier.Apply(context, typeValue);
        }
        
        public bool GetMemberVarValue(
            TypeValue parentTypeValue, 
            Name varName,
            [NotNullWhen(true)] out VarValue? outVarValue)
        {
            outVarValue = null;

            var parentNTV = parentTypeValue as TypeValue.Normal;
            if (parentNTV == null) return false;

            var typeId = parentNTV.GetTypeId();
            var typeInfo = itemInfoRepo.GetItem<TypeInfo>(typeId);
            if (typeInfo == null)
                return false;

            var varInfo = typeInfo.GetVar(varName);

            if (varInfo == null)
                return false;

            outVarValue = new VarValue(parentNTV.ModuleName, parentNTV.NamespacePath, parentNTV.GetAllEntries(), varName);
            return true;
        }
    }
}
