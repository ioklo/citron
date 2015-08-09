using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Test.Metadata
{
    class TopNamespace : Namespace
    {
        public TopNamespace()
            : base(null, "") { }

        internal override string GetChildFullName(string name)
        {
            return name;
        }
    }
}
