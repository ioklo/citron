using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
{
    public class VariableExp : IExp
    {
        public string Name { get; set; }
        public List<IOffset> Offsets { get; private set; }

        // type checker fill this
        public List<int> IndexOffsets { get; private set; }

        public VariableExp()
        {
            Offsets = new List<IOffset>();
            IndexOffsets = new List<int>();
        }

        public void Visit(IExpVisitor visitor)
        {
            visitor.Visit(this);
        }

        public Result Visit<Result>(IExpVisitor<Result> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
