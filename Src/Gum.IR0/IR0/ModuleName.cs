using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct ModuleName
    {
        public string Value { get; }
        public static implicit operator ModuleName(string s) => new ModuleName(s);
    }
}
