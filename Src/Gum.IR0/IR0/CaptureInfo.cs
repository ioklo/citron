using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;

namespace Gum.IR0
{
    public class CaptureInfo
    {
        public abstract class Element
        {
        }

        public class CopyLocalElement : Element
        {
            public TypeId TypeId { get; }
            public string LocalVarName { get; }

            public CopyLocalElement(TypeId typeId, string localVarName)
            {
                TypeId = typeId;
                LocalVarName = localVarName;
            }
        }

        public class RefLocalElement : Element
        {
            public string LocalVarName { get; }

            public RefLocalElement(string localVarName)
            {
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
