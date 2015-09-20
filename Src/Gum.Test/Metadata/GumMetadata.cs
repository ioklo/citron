using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Test.Metadata
{
    class GumMetadata : IMetadata
    {
        // 최상위 네임스페이스 
        public TopNamespace TopNamespace { get; private set; }

        public GumMetadata()
        {
            TopNamespace = new TopNamespace();
        }

        public Namespace GetNamespace(IReadOnlyList<string> names)
        {
            Namespace curNS = TopNamespace;

            foreach(var name in names)
            {
                curNS = curNS.GetNamespace(name);
                if (curNS == null) return null;
            }

            return curNS;
        }
    }
}
