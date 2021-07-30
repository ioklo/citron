using Gum.Collections;
using Gum.CompileTime;
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
    partial class ConstructorValue : CallableValue
    {
        ItemValueFactory itemValueFactory;

        // X<int>.Y<short>
        ItemValueOuter outer;
        IModuleConstructorInfo info;

        public override ItemValue Apply_ItemValue(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);            
            return itemValueFactory.MakeConstructor(appliedOuter, info);
        }

        protected override TypeValue MakeTypeValueByMType(M.Type type)
        {
            return itemValueFactory.MakeTypeValueByMType(type);
        }

        protected override ImmutableArray<Param> GetParameters()
        {
            return info.GetParameters();
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
