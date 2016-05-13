using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Metadatas
{
    abstract class MetadataComponent
    {
        public Namespace Namespace { get; private set; }
        public string Name { get; private set; }
        public string FullName { get { return Namespace.GetChildFullName(Name); } }

        public MetadataComponent(Namespace ns, string name)
        {
            Namespace = ns;
            Name = name;
        }        
    }
}
