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

        internal override int FillTypeEnv(TypeEnvBuilder builder)
        {
            int depth;
            if (outer != null)
                depth = outer.FillTypeEnv(builder) + 1;
            else
                depth = 0;

            for(int i = 0; i < typeArgs.Length; i++)
                builder.Add(depth, i, typeArgs[i]);

            return depth;
        }
        
        // class X<T> { void Func<U>(T t, U u, int x); }
        // X<int>.F<bool> => (int, bool, int)
        public ImmutableArray<TypeValue> GetParamTypes()
        {
            var typeEnv = MakeTypeEnv();

            var builder = ImmutableArray.CreateBuilder<TypeValue>(funcInfo.ParamTypes.Length);
            foreach (var paramType in funcInfo.ParamTypes)
            {   
                var paramTypeValue = itemValueFactory.MakeTypeValue(paramType);
                var appliedParamTypeValue = paramTypeValue.Apply(typeEnv);
                builder.Add(appliedParamTypeValue);
            }

            return builder.MoveToImmutable();
        }

        public TypeValue GetRetType()
        {
            var typeEnv = MakeTypeEnv();
            var retTypeValue = itemValueFactory.MakeTypeValue(funcInfo.RetType);
            return retTypeValue.Apply(typeEnv);
        }
        
        // IR0 Func를 만들어 줍니다
        public override R.Path GetRType()
        {
            var rname = RItemFactory.MakeName(funcInfo.Name);
            var paramTypes = GetParamTypes();
            var rparamTypes = ImmutableArray.CreateRange(paramTypes, paramType => paramType.GetRType());

            var paramHash = new R.ParamHash(typeArgs.Length, rparamTypes);

            var rtypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.GetRType());
            return outer.MakeRPath(rname, paramHash, rtypeArgs);
        }   
    }
}
