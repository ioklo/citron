using System;
using R = Gum.IR0;

namespace Gum.Analysis
{
    public abstract record EnumSymbolOuter
    {
        public abstract TypeEnv GetTypeEnv();
        public abstract R.Path.Normal MakeRPath();
        public abstract EnumSymbolOuter Apply(TypeEnv typeEnv);
    }
}