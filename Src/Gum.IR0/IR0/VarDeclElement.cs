using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0
{
    public class VarDeclElement
    {
        public string Name { get; }
        public Type Type { get; }
        public Exp? InitExp { get; }

        public VarDeclElement(string name, Type type, IR0.Exp? initExp)
        {
            Name = name;
            Type = type;
            InitExp = initExp;
        }
    }
}
