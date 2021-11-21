using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;
using System;
using Gum.Infra;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial struct ParamInfo
    {
        public R.ParamKind ParamKind { get; }
        public TypeValue Type { get; }

        public ParamInfo Apply(TypeEnv typeEnv)
        {
            return new ParamInfo(ParamKind, Type.Apply_TypeValue(typeEnv));
        }
    }
}
