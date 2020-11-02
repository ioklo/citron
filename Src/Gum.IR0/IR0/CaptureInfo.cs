using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;

namespace Gum.IR0
{
    public class CaptureInfo
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

        public CaptureInfo(bool bCaptureThis, IEnumerable<Element> captures)
        {
            this.bShouldCaptureThis = bCaptureThis;
            this.Captures = captures.ToImmutableArray();
        }
    }
}
