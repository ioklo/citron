using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Core.IL;

namespace Gum.App.VM
{
    // Custom object ^^
    class Object
    {
        // does it need to have a type information? no, not now.
        public List<object> Fields {get; private set;}

        public Object(int n)
        {
            Fields = new List<object>();
            Fields.AddRange(Enumerable.Repeat<object>(null, n));
        }
    }
}
