using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Translator.Parser
{
    public abstract class Symbol
    {
        public string ID { get; private set; }
        public Symbol(string id) { ID = id; }

        public override string ToString()
        {
            return ID;
        }
    }
}
