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
    // deprecated, 안의 내용을 보존하지 않아도 된다
    class TypeValueService
    {
        ITypeInfoRepository typeInfoRepo;
        TypeValueApplier typeValueApplier;

        public TypeValueService(ITypeInfoRepository typeInfoRepo, TypeValueApplier typeValueApplier)
        {
            this.typeInfoRepo = typeInfoRepo;
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

        //    var typeValue_normal = typeValue as QsNormalTypeValue;
        //    if (typeValue_normal == null) return false;

        //    if (!GetMemberVarInfo(typeValue_normal.TypeId, name, out var memberVarInfo))
        //        return false;

        //    outTypeValue = typeValueApplier.Apply(typeValue_normal, memberVarInfo.TypeValue);
        //    return true;
        //}    
        
        // class X<T> { class Y<U> { Dict<T, U> x; } } 
        // GetTypeValue(X<int>.Y<short>, x) => Dict<int, short>
        public TypeValue GetTypeValue(MemberVarValue varValue)
        {
            return typeValueApplier.Apply(varValue.outer, varValue.TypeValue);
        }

        // class X<T> { class Y<U> { S<T> F<V>(V v, List<U> u); } } => MakeFuncTypeValue(X<int>.Y<short>, F, context) 
        // (V, List<short>) => S<int>
        public FuncTypeValue GetTypeValue(FuncValue funcValue)
        {
            // var funcInfo = typeInfoRepo.GetItem<FuncInfo>(funcValue.GetFuncId());
            Debug.Assert(funcInfo != null);

            // 
            TypeValue retTypeValue;
            if (funcValue.IsSequence)
            {
                retTypeValue = new NormalTypeValue(ItemIds.Enumerable, new[] { new[] { funcInfo.RetTypeValue } });
            }
            else
            {
                retTypeValue = funcValue.RetType;
            }

            return typeValueApplier.Apply_Func(funcValue, new FuncTypeValue(retTypeValue, funcInfo.ParamTypeValues));
        }

        // 
        // GetFuncTypeValue_NormalTypeValue(X<int>.Y<short>, "Func", <bool>) =>   (int, short) => bool
        // 
        //private bool GetMemberFuncTypeValue_Normal(
        //    bool bStaticOnly,
        //    QsNormalTypeValue typeValue,
        //    QsName memberFuncId,
        //    ImmutableArray<QsTypeValue> typeArgs,
        //    [NotNullWhen(true)] out QsFuncTypeValue? funcTypeValue)
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
        //    [NotNullWhen(true)] out QsFuncTypeValue? funcTypeValue)
        //{
        //    // var / typeVar / normal / func

        //    if (typeValue is QsNormalTypeValue typeValue_normal)
        //        return GetMemberFuncTypeValue_Normal(bStaticOnly, typeValue_normal, memberFuncId, typeArgs, out funcTypeValue);

        //    throw new NotImplementedException();
        //}

        // class N<T> : B<T> => N.GetBaseType => B<T(N)>
        private bool GetBaseTypeValue_Normal(NormalTypeValue typeValue, out TypeValue? outBaseTypeValue)
        {
            outBaseTypeValue = null;

            var typeId = typeValue.GetTypeId();
            var typeInfo = typeInfoRepo.GetItem<TypeInfo>(typeId);
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
                NormalTypeValue normalTypeValue => GetBaseTypeValue_Normal(normalTypeValue, out baseTypeValue),
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

            NormalTypeValue? ntv = objTypeValue as NormalTypeValue;
            if (ntv == null) return false;

            var typeInfo = typeInfoRepo.GetItem<TypeInfo>(ntv.GetTypeId());
            if (typeInfo == null)
                return false;

            var funcInfo = typeInfo.GetFuncs(funcName).SingleOrDefault();

            if (funcInfo == null)
                return false;

            // 함수는 typeArgs가 모자라도 최대한 매칭한다
            if (funcInfo.TypeParams.Length < typeArgs.Count)
                return false;

            int i = typeArgs.Count;
            var completedTypeArgs = typeArgs.Concat(
                funcInfo.TypeParams.Skip(typeArgs.Count).Select(typeParam => {
                    var typeVar = new TypeVarTypeValue(funcInfo.GetId().OuterEntries.Length, i, typeParam);
                    i++;
                    return typeVar;
                })
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
        
        // class X<T> { class Y<U> { Dict<T, U> x; } }
        // GetMemberVarValue(X<int>.Y<short>, x) => MemberVarValue(X<int>.Y<short>, x, Dict<int, short>)
        public MemberVarValue? GetMemberVarValue(TypeValue outer, Name name)
        {
            var outerNTV = outer as NormalTypeValue;
            if (outerNTV == null) return null;

            var typeId = outerNTV.GetTypeId();
            var typeInfo = typeInfoRepo.GetType(typeId);
            if (typeInfo == null)
                return null;

            var varInfo = typeInfo.GetVar(name);

            if (varInfo == null)
                return null;

            return new MemberVarValue(outerNTV, memberVarInfo);            
        }
    }
}
