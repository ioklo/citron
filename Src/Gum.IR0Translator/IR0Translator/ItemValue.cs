﻿using Pretune;
using R = Gum.IR0;
using M = Gum.CompileTime;
using Gum.Collections;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    // TypeValue, FuncValue, MemberVarValue
    abstract class ItemValue
    {
        internal virtual int FillTypeEnv(TypeEnvBuilder builder) { return 0; }

        protected TypeEnv MakeTypeEnv()
        {
            // TypeContext 빌더랑 똑같이 생긴
            var builder = new TypeEnvBuilder();
            FillTypeEnv(builder);
            return builder.Build();
        }

        public abstract R.Path GetRType();
    }    
    
}
