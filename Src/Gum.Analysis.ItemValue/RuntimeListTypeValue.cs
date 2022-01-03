using System;
using static Gum.Infra.Misc;
using R = Gum.IR0;
using Pretune;

namespace Gum.Analysis
{
    // 런타임 라이브러리로 구현할 리스트 타입
    [AutoConstructor]
    public partial class RuntimeListTypeValue : TypeSymbol
    {
        public ItemValueFactory itemValueFactory;
        public TypeSymbol ElemType { get; }

        public override TypeSymbol Apply(TypeEnv typeEnv)
        {
            var appliedElemType = ElemType.Apply(typeEnv);
            return itemValueFactory.MakeListType(appliedElemType);
        }

        public override R.Path GetRPath()
        {
            var runtime = new R.Path.Root("System.Runtime");
            var runtimeSystem = new R.Path.Nested(runtime, new R.Name.Normal("System"), R.ParamHash.None, default);
            var runtimeSystemList = new R.Path.Nested(runtimeSystem, new R.Name.Normal("List"), new R.ParamHash(1, default), Arr(ElemType.GetRPath()));

            return runtimeSystemList;
        }
        
        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
        {
            throw new NotImplementedException();
        }

        public R.Path GetIterRPath()
        {
            var runtime = new R.Path.Root("System.Runtime");
            var runtimeSystem = new R.Path.Nested(runtime, new R.Name.Normal("System"), R.ParamHash.None, default);
            var runtimeSystemList = new R.Path.Nested(runtimeSystem, new R.Name.Normal("List"), new R.ParamHash(1, default), Arr(ElemType.GetRPath()));
            var runtimeSystemListIter = new R.Path.Nested(runtimeSystemList, new R.Name.Anonymous(0), R.ParamHash.None, default);

            return runtimeSystemListIter;
        }

        public override int GetTotalTypeParamCount()
        {
            return 1;
        }
    }
}
