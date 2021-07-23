using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    [AutoConstructor]
    partial class ConstructorValue : ItemValue
    {
        ItemValueFactory itemValueFactory;

        // X<int>.Y<short>
        ItemValueOuter outer;
        M.ConstructorInfo info;

        public override ItemValue Apply_ItemValue(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);            
            return itemValueFactory.MakeConstructor(appliedOuter, info);
        }

        ImmutableArray<ParamInfo> GetParamInfos()
        {
            var typeEnv = MakeTypeEnv();

            var builder = ImmutableArray.CreateBuilder<ParamInfo>(info.Parameters.Length);
            foreach (var paramInfo in info.Parameters)
            {
                var paramTypeValue = itemValueFactory.MakeTypeValueByMType(paramInfo.Type);
                var appliedParamTypeValue = paramTypeValue.Apply_TypeValue(typeEnv);

                var paramKind = paramInfo.Kind switch
                {
                    M.ParamKind.Normal => R.ParamKind.Normal,
                    M.ParamKind.Ref => R.ParamKind.Ref,
                    M.ParamKind.Params => R.ParamKind.Params,
                    _ => throw new UnreachableCodeException()
                };

                builder.Add(new ParamInfo(paramKind, appliedParamTypeValue));
            }

            return builder.MoveToImmutable();
        }


        public sealed override R.Path GetRPath()
        {
            return GetRPath_Nested();
        }

        public R.Path.Nested GetRPath_Nested()
        {
            var paramInfos = GetParamInfos();

            var rparamTypes = ImmutableArray.CreateRange(paramInfos, paramInfo => new R.ParamHashEntry(paramInfo.ParamKind, paramInfo.Type.GetRPath()));

            var paramHash = new R.ParamHash(0, rparamTypes);
            
            return outer.GetRPath(R.Name.Constructor.Instance, paramHash, default);
        }
    }
}
