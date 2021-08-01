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
    partial class ConstructorValue : CallableValue
    {
        ItemValueFactory itemValueFactory;

        // X<int>.Y<short>
        NormalTypeValue outer;
        IModuleConstructorInfo info;

        public override ItemValue Apply_ItemValue(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply_NormalTypeValue(typeEnv);            
            return itemValueFactory.MakeConstructor(appliedOuter, info);
        }

        protected override TypeValue MakeTypeValueByMType(M.Type type)
        {
            return itemValueFactory.MakeTypeValueByMType(type);
        }

        protected override ImmutableArray<M.Param> GetParameters()
        {
            return info.GetParameters();
        }

        public sealed override R.Path GetRPath()
        {
            return GetRPath_Nested();
        }

        public bool CheckAccess(NormalTypeValue? thisType)
        {
            var accessModifier = info.GetAccessModifier();

            switch (accessModifier)
            {
                case M.AccessModifier.Public: return true;
                case M.AccessModifier.Protected: throw new NotImplementedException();
                case M.AccessModifier.Private:
                    {
                        // NOTICE: MemberVarValue에도 같은 코드가 있다
                        if (thisType == null) return false;
                        
                        var memberContainerPath = outer.GetRPath_Nested();

                        // path로 비교할 수 있을거 같다
                        R.Path.Nested? curPath = thisType.GetRPath_Nested();
                        while (curPath != null)
                        {
                            // TypeArgs는 빼고 비교한다
                            if (memberContainerPath.Outer.Equals(curPath.Outer) &&
                                memberContainerPath.Name.Equals(curPath.Name) &&
                                memberContainerPath.ParamHash.Equals(curPath.ParamHash))
                                return true;

                            curPath = curPath.Outer as R.Path.Nested;
                        }

                        return false;
                    }

                default: throw new UnreachableCodeException();
            }

        }

        public R.Path.Nested GetRPath_Nested()
        {
            var paramInfos = GetParamInfos();

            var rparamTypes = ImmutableArray.CreateRange(paramInfos, paramInfo => new R.ParamHashEntry(paramInfo.ParamKind, paramInfo.Type.GetRPath()));

            var paramHash = new R.ParamHash(0, rparamTypes);

            return new R.Path.Nested(outer.GetRPath_Nested(), R.Name.Constructor.Instance, paramHash, default);            
        }

        public override int GetTotalTypeParamCount()
        {
            return outer.GetTotalTypeParamCount(); // NOTICE: typeParameter를 가질 수 없으므로
        }
    }
}
