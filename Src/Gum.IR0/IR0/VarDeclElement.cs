using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class VarDeclElement
    {
        public string Name { get; }
        public Type Type { get; }
        public Exp? InitExp { get; }
    }
}
