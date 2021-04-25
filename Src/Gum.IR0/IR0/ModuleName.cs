using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct ModuleName
    {
        string value;
        public static implicit operator ModuleName(string s) => new ModuleName(s);
    }
}
