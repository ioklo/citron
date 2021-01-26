using Pretune;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.CompileTime
{
    [ImplementIEquatable]
    public partial struct ModuleName
    {
        string Text { get; }

        private ModuleName(string text)
        {
            Text = text;
        }

        public static implicit operator ModuleName(string name)
        {
            return new ModuleName(name);
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
