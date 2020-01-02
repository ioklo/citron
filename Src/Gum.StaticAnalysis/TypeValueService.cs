using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Gum.CompileTime;

namespace Gum
{
    public class TypeValueService
    {
        ModuleInfoService moduleInfoService;
        TypeValueApplier typeValueApplier;

        public TypeValueService(ModuleInfoService moduleInfoService, TypeValueApplier typeValueApplier)
        {
            this.moduleInfoService = moduleInfoService;
            this.typeValueApplier = typeValueApplier;
        }

        //private bool GetMemberVarInfo(QsMetaItemId typeId, QsName name, [NotNullWhen(returnValue: true)] out QsVarInfo? outVarInfo)
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

        //public bool GetMemberVarTypeValue(QsTypeValue typeValue, QsName name, [NotNullWhen(returnValue: true)] out QsTypeValue? outTypeValue)
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
            var varInfo = moduleInfoService.GetVarInfos(varValue.VarId).Single();

            if (varInfo.OuterId != null)
                return typeValueApplier.Apply(TypeValue.MakeNormal(varInfo.OuterId, varValue.OuterTypeArgList), varInfo.TypeValue);
            else
                return varInfo.TypeValue;
        }

        // class X<T> { class Y<U> { S<T> F<V>(V v, List<U> u); } } => MakeFuncTypeValue(X<int>.Y<short>, F, context) 
        // (V, List<short>) => S<int>
        public TypeValue.Func GetTypeValue(FuncValue funcValue)
        {
            var funcInfo = moduleInfoService.GetFuncInfos(funcValue.FuncId).Single();

            // 
            TypeValue retTypeValue;
            if (funcInfo.bSeqCall)
            {
                var enumerableId = ModuleItemId.Make("Enumerable", 1);
                retTypeValue = TypeValue.MakeNormal(enumerableId, TypeArgumentList.Make(funcInfo.RetTypeValue));
            }
            else
            {
                retTypeValue = funcInfo.RetTypeValue;
            }

            return typeValueApplier.Apply_Func(funcValue, TypeValue.MakeFunc(retTypeValue, funcInfo.ParamTypeValues));
        }


        // 
        // GetFuncTypeValue_NormalTypeValue(X<int>.Y<short>, "Func", <bool>) =>   (int, short) => bool
        // 
        //private bool GetMemberFuncTypeValue_Normal(
        //    bool bStaticOnly,
        //    QsTypeValue.Normal typeValue,
        //    QsName memberFuncId,
        //    ImmutableArray<QsTypeValue> typeArgs,
        //    [NotNullWhen(returnValue: true)] out QsTypeValue.Func? funcTypeValue)
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
        //    [NotNullWhen(returnValue: true)] out QsTypeValue.Func? funcTypeValue)
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

            var typeInfo = moduleInfoService.GetTypeInfos(typeValue.TypeId).SingleOrDefault();
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
            [NotNullWhen(returnValue: true)] out FuncValue? funcValue)
        {
            funcValue = null;

            TypeValue.Normal? ntv = objTypeValue as TypeValue.Normal;
            if (ntv == null) return false;

            var typeInfo = moduleInfoService.GetTypeInfos(ntv.TypeId).SingleOrDefault();
            if (typeInfo == null)
                return false;

            if (!typeInfo.GetMemberFuncId(funcName, out var memberFuncId))
                return false;

            var funcInfo = moduleInfoService.GetFuncInfos(memberFuncId).SingleOrDefault();

            if (funcInfo == null)
                return false;

            // 함수는 typeArgs가 모자라도 최대한 매칭한다
            if (funcInfo.TypeParams.Length < typeArgs.Count)
                return false;

            var completedTypeArgs = typeArgs.ToList();

            for (int i = typeArgs.Count; i < funcInfo.TypeParams.Length; i++)
                completedTypeArgs.Add(TypeValue.MakeTypeVar(memberFuncId, funcInfo.TypeParams[i]));

            funcValue = new FuncValue(memberFuncId, TypeArgumentList.Make(ntv.TypeArgList, completedTypeArgs));
            return true;
        }

        public TypeValue Apply(TypeValue context, TypeValue typeValue)
        {
            return typeValueApplier.Apply(context, typeValue);
        }

        public bool GetMemberVarValue(
            TypeValue objTypeValue, 
            Name varName,
            [NotNullWhen(returnValue: true)] out VarValue? outVarValue)
        {
            outVarValue = null;

            var ntv = objTypeValue as TypeValue.Normal;
            if (ntv == null) return false;

            var typeInfo = moduleInfoService.GetTypeInfos(ntv.TypeId).SingleOrDefault();
            if (typeInfo == null)
                return false;

            if (!typeInfo.GetMemberVarId(varName, out var memberVarId))
                return false;

            outVarValue = new VarValue(memberVarId, ntv.TypeArgList);
            return true;
        }
    }
}
