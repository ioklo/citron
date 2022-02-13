using Gum.CompileTime;
using Pretune;
using System;
using System.Collections.Generic;
using System.Text;

namespace Citron.IR0
{
    public abstract class StringExpElement
    {
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class TextStringExpElement : StringExpElement
    {
        public string Text { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class ExpStringExpElement : StringExpElement
    {
        public Exp Exp { get; }
    }
}
