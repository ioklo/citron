using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Metadatas
{
    // Gum 파일로부터 만들어지는 메타데이타
    class GumMetadata : IMetadata
    {
        // 지금 현재 쓰이는 Namespace들
        Dictionary<string, Namespace> idNamespaceMap = new Dictionary<string, Namespace>();
        // List<MetadataComponent> comps; 

        public GumMetadata()
        {
        }

        private Namespace GetOrCreateNamespace(string namespaceID)
        {
            Namespace ns;
            if (!idNamespaceMap.TryGetValue(namespaceID, out ns))
            {
                ns = new Namespace(ns, namespaceID);
                idNamespaceMap.Add(namespaceID, ns);
            }

            return ns;
        }

        internal void AddClassDef(string namespaceID, string name, int typeArgCount)
        {
            var ns = GetOrCreateNamespace(namespaceID);
            // comps.Add(new TypeDef(ns, name, typeArgCount));
        }

        internal void AddStructDef(string namespaceID, string name, int typeArgCount)
        {
            var ns = GetOrCreateNamespace(namespaceID);
            // comps.Add(new TypeDef(ns, name, typeArgCount));
        }
    }
}
