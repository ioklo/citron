using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Test.Metadata
{
    interface IMetadata
    {
        Namespace GetNamespace(IReadOnlyList<string> readOnlyList);
    }
}
