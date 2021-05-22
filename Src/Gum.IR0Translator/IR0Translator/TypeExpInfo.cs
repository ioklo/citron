using Pretune;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    // X<int>.Y<T>, closed
    abstract record TypeExpInfo;

    record MTypeTypeExpInfo(M.Type Type) : TypeExpInfo;
    record EnumElemTypeExpInfo(M.Type EnumType, string ElemName) : TypeExpInfo;
    record VarTypeExpInfo : TypeExpInfo
    {
        public static readonly VarTypeExpInfo Instance = new VarTypeExpInfo();
        VarTypeExpInfo() { }
    }
}
