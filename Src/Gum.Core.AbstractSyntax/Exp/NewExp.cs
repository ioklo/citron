using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public class NewExp : IExp
    {
        public string TypeName { get; private set; }
        public IReadOnlyList<string> TypeArgs { get; private set; }
        public IReadOnlyList<IExp> Args { get; private set; }

        public NewExp(string typeName, IEnumerable<string> typeArgs, IEnumerable<IExp> args)
        {
            TypeName = typeName;
            TypeArgs = typeArgs.ToList();
            Args = args.ToList();
        }

        public void Visit(IExpVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
