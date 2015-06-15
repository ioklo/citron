using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public enum BinaryExpKind
    {
        Invalid,

        Equal, // Bool, String, Integer
        NotEqual, // Bool, String, Integer

        And,   // Bool
        Or,    // Bool

        Add,   // Integer, String
        Sub,   // Integer
        Mul,   // Integer
        Div,   // Integer
        Mod,   // Integer 

        Less,  // Integer
        Greater, // Integer
        LessEqual, // Integer
        GreaterEqual, // Integer
    }
}
