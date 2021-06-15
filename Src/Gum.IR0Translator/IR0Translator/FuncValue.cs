using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Infra;
using Gum.Collections;

namespace Gum.IR0Translator
{
    // X<int>.Y<short>.F_T_int_int<S>
    class FuncValue : ItemValue
    {
        ItemValueFactory itemValueFactory;

        // X<int>.Y<short>
        ItemValueOuter outer;

        // F_int_int
        M.FuncInfo funcInfo;

        ImmutableArray<TypeValue> typeArgs;

        public bool IsStatic { get => !funcInfo.IsInstanceFunc; }
        public bool IsSequence { get => funcInfo.IsSequenceFunc; }
        
        internal FuncValue(ItemValueFactory itemValueFactory, ItemValueOuter outer, M.FuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
        {
            this.itemValueFactory = itemValueFactory;            
            this.outer = outer;
            
            this.funcInfo = funcInfo;
            this.typeArgs = typeArgs;
        }

        internal override void FillTypeEnv(TypeEnvBuilder builder)
        {
            if (outer != null)
                outer.FillTypeEnv(builder);

            for(int i = 0; i < typeArgs.Length; i++)
                builder.Add(typeArgs[i]);
        }
        
        // class X<T> { void Func<U>(T t, U u, int x); }
        // X<int>.F<bool> => (int, bool, int)
        public ImmutableArray<TypeValue> GetParamTypes()
        {
            var typeEnv = MakeTypeEnv();

            var builder = ImmutableArray.CreateBuilder<TypeValue>(funcInfo.Parameters.Length);
            foreach (var paramInfo in funcInfo.Parameters)
            {   
                var paramTypeValue = itemValueFactory.MakeTypeValueByMType(paramInfo.Type);
                var appliedParamTypeValue = paramTypeValue.Apply_TypeValue(typeEnv);
                builder.Add(appliedParamTypeValue);
            }

            return builder.MoveToImmutable();
        }

        public TypeValue GetRetType()
        {
            var typeEnv = MakeTypeEnv();
            var retTypeValue = itemValueFactory.MakeTypeValueByMType(funcInfo.RetType);
            return retTypeValue.Apply_TypeValue(typeEnv);
        }
        
        // IR0 Func를 만들어 줍니다
        public sealed override R.Path GetRPath()
        {
            return GetRPath_Nested();
        }

        public override ItemValue Apply_ItemValue(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply_TypeValue(typeEnv));
            return itemValueFactory.MakeFunc(appliedOuter, funcInfo, appliedTypeArgs);
        }

        public R.Path.Nested GetRPath_Nested()
        {
            var rname = RItemFactory.MakeName(funcInfo.Name);
            var paramTypes = GetParamTypes();
            var rparamTypes = ImmutableArray.CreateRange(paramTypes, paramType => paramType.GetRPath());

            var paramHash = new R.ParamHash(typeArgs.Length, rparamTypes);

            var rtypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.GetRPath());
            return outer.GetRPath(rname, paramHash, rtypeArgs);
        }
    }
}
