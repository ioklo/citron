using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
{
    public class IndexOffset : IOffset
    {
        public int Index { get; set; }
    }
}
