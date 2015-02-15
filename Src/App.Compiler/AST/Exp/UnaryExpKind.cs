﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
{
    public enum UnaryExpKind
    {
        Invalid,
        Neg, // '-' Integer
        Not, // !   Bool
        PrefixInc,  // ++i
        PrefixDec,  // --i
        PostfixInc, // i++
        PostfixDec, // i--
    }
}
