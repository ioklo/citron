using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.IR0
{
    public abstract class StringExpElement
    {
    }

    public class TextStringExpElement : StringExpElement
    {
        public string Text { get; }
        public TextStringExpElement(string text) { Text = text; }
    }

    public class ExpStringExpElement : StringExpElement
    {
        public Exp Exp { get; }
        public ExpStringExpElement(Exp exp) { Exp = exp; }
    }
}
