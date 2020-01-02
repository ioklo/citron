using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Gum.StaticAnalysis
{   
    public class CaptureInfo
    {
        public struct Element
        {   
            public CaptureKind CaptureKind { get; }
            public StorageInfo StorageInfo { get; }

            public Element(CaptureKind captureKind, StorageInfo storageInfo)
            {
                CaptureKind = captureKind;
                StorageInfo = storageInfo;
            }
        }

        public bool bCaptureThis { get; }
        public ImmutableArray<Element> Captures { get; }

        public CaptureInfo(bool bCaptureThis, ImmutableArray<Element> captures)
        {
            this.bCaptureThis = bCaptureThis;
            this.Captures = captures;
        }
    }
}
