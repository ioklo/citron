using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct ParamInfo
    {
        public Path Type { get; }
        public string Name { get; }
    }
}
