using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using M = Gum.CompileTime;

namespace Gum.IR0
{   
    class TypeEnvFactory
    {
        ITypeInfoRepository typeInfoRepo;

        public TypeEnvFactory(ITypeInfoRepository typeInfoRepo)
        {
            this.typeInfoRepo = typeInfoRepo;
        }

        void FillTypeEnv(int depth, ImmutableArray<string> typeParams, ImmutableArray<TypeValue> typeArgs, Dictionary<TypeVarTypeValue, TypeValue> typeEnv)
        {
            Debug.Assert(typeParams.Length == typeArgs.Length);

            for (int i = 0; i < typeParams.Length; i++)
                typeEnv[new TypeVarTypeValue(depth, i, typeParams[i])] = typeArgs[i];
        }

        // class X<T> { class Y<T> { } } 일때
        // FillTypeEnv(X<int>.Y<short>, env) -> env with {Tx -> int, Ty -> short}
        (int Depth, M.TypeInfo ParentTypeInfo) FillTypeEnv(NormalTypeValue ntv, Dictionary<TypeVarTypeValue, TypeValue> typeEnv)
        {
            // base case 
            // [Module]Namespace.X
            var outer = ntv.GetOuter();
            if (outer == null)
            {
                var typeInfo = typeInfoRepo.GetType(ntv.GetTypeId());
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


        public TypeEnv Make(TypeValue typeValue)
        {
            var outer = typeValue.GetOuter();

            Dictionary<TypeVarTypeValue, TypeValue> typeEnv;
            FillTypeEnv(outer, typeEnv);
        }
    }

    interface ITypeEnv
    {
        TypeValue GetValue(int depth, int index);
    }
    
    class TypeEnv : ITypeEnv
    {
        ImmutableDictionary<TypeVarTypeValue, TypeValue> typeEnv;
        int depth;

        public TypeEnv(TypeValueFactory factory, ImmutableArray<string> typeParams, ImmutableArray<TypeValue> typeArgs)
        {
            Debug.Assert(typeParams.Length == typeArgs.Length);
            this.depth = 0;

            var builder = ImmutableDictionary.CreateBuilder<TypeVarTypeValue, TypeValue>();

            for (int i = 0; i < typeParams.Length; i++)
            {
                var key = factory.MakeTypeVar(depth, i, typeParams[i]);
                var value = typeArgs[i];

                builder.Add(key, value);
            }

            this.typeEnv = builder.ToImmutable();
        }

        // TypeEnv 상태에서 [T00->int] 
        // ApplyTypeNormal([Ty => int, U => short], ((X<T>, [Tx => Ty]).Y<short>, [Uy => short]).List<U>) => X<int>, (T => int).Y<short>.List<short>)
        NormalTypeValue ApplyTypeNormal(NormalTypeValue ntv)
        {
            return ntv.ReplaceTypeVars(typeEnv);
            
            // 여기서 재 조립 가능한가
            ntv.Make(typeValueFactory, outer, )

            var appliedOuterEntries = ntv.OuterEntries.Select(outerEntry =>
                new AppliedItemPathEntry(outerEntry.Name, outerEntry.ParamHash, outerEntry.TypeArgs.Select(typeArg => Apply(typeArg, typeEnv)))
            );

            var appliedEntry = new AppliedItemPathEntry(ntv.Entry.Name, ntv.Entry.ParamHash, ntv.Entry.TypeArgs.Select(typeArg => Apply(typeArg, typeEnv)));

            return new NormalTypeValue(ntv.ModuleName, ntv.NamespacePath, appliedOuterEntries, appliedEntry);
        }

        // 
        FuncTypeValue ApplyFunc(FuncTypeValue typeValue)
        {
            return new FuncTypeValue(
                Apply(typeValue.Return, typeEnv),
                typeValue.Params.Select(parameter => Apply(parameter, typeEnv)));
        }

        // T, [T -> ]
        TypeValue ApplyTypeVar(TypeVarTypeValue typeValue)
        {
            if (typeEnv.TryGetValue(typeValue, out var appliedTypeValue))
                return appliedTypeValue;

            return typeValue;
        }

        public TypeValue Apply(TypeValue typeValue)
        {
            switch(typeValue)
            { 
                case NormalTypeValue normalTypeValue: return ApplyTypeNormal(normalTypeValue);
                case FuncTypeValue funcTypeValue: return ApplyFunc(funcTypeValue);
                case TypeVarTypeValue typeVarTypeValue: return ApplyTypeVar(typeVarTypeValue);
                case VoidTypeValue vtv: return vtv;
                case EnumElemTypeValue _: throw new NotImplementedException();
                default: throw new UnreachableCodeException();
            }
        }

        // class X<T> { class Y<U> { S<T>.List<U> u; } } => ApplyTypeValue_Normal(X<int>.Y<short>, S<T>.List<U>) => S<int>.Dict<short>        

        // 주어진 funcValue 컨텍스트 내에서, typeValue를 치환하기
        public FuncTypeValue Apply(FuncTypeValue typeValue)
        {
            var outer = context.GetOuter();
            if (outer == null)
            {
                var funcInfo = typeInfoRepo.GetItem<FuncInfo>(context.GetFuncId());
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

            return ApplyFunc(typeValue, typeEnv);
        }

        public TypeValue GetValue(int depth, int index)
        {
            
        }
    }
}