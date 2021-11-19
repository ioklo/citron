using Pretune;
using R = Gum.IR0;
using M = Gum.CompileTime;
using Gum.Collections;
using System.Diagnostics;
using System;

namespace Gum.Analysis
{
    // TypeValue, FuncValue, MemberVarValue
    public abstract class ItemValue
    {
        internal virtual void FillTypeEnv(TypeEnvBuilder builder) { }

        public TypeEnv MakeTypeEnv()
        {
            var builder = new TypeEnvBuilder();
            FillTypeEnv(builder);
            return builder.Build();
        }

        public abstract R.Path GetRPath();
        public abstract ItemValue Apply_ItemValue(TypeEnv typeEnv);
        public abstract int GetTotalTypeParamCount();
    }    
}
