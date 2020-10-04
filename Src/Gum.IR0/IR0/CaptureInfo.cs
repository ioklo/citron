using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;

namespace Gum.IR0
{
    public class CaptureInfo
    {
        public struct Element
        {
            public CaptureKind CaptureKind { get; }
            public int LocalVarIndex { get; }

            public Element(CaptureKind captureKind, int localVarIndex)
            {
                CaptureKind = captureKind;
                LocalVarIndex = localVarIndex;
            }
        }

        public bool bCaptureThis { get; }
        public ImmutableArray<Element> Captures { get; }

        public CaptureInfo(bool bCaptureThis, IEnumerable<Element> captures)
        {
            this.bCaptureThis = bCaptureThis;
            this.Captures = captures.ToImmutableArray();
        }
    }
}
