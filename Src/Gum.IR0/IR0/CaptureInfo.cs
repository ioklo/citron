using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class CaptureInfo
    {
        public class Element
        {
            public Type Type { get; }
            public string LocalVarName { get; }

            public Element(Type type, string localVarName)
            {
                Type = type;
                LocalVarName = localVarName;
            }
        }

        public bool bShouldCaptureThis { get; }
        public ImmutableArray<Element> Captures { get; }
    }
}
