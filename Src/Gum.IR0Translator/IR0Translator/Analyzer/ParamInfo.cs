using M = Gum.CompileTime;
using Pretune;
using System;

namespace Gum.IR0Translator
{
    [AutoConstructor]
    partial struct ParamInfo
    {
        public M.ParamKind ParamKind { get; }
        public TypeValue Type { get; }

        public ParamInfo Apply(TypeEnv typeEnv)
        {
            return new ParamInfo(ParamKind, Type.Apply_TypeValue(typeEnv));
        }
    }
}
