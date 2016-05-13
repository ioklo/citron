using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite.Generator
{
    class StructElem : BaseElement
    {
        List<ElementVar> variables = new List<ElementVar>();
        public IReadOnlyList<ElementVar> Variables 
        {
            get 
            {
                var list = new List<ElementVar>();
                
                foreach (var comp in Comps)
                    list.AddRange(comp.Variables);

                list.AddRange(variables);

                return list;
            }
        }

        public StructElem(string name)
            : base(name)
        {
        }

        internal StructElem Var(BaseElement element, string name)
        {
            variables.Add(new ElementVar(VarType.Single, element, name, false));
            return this;
        }

        internal StructElem VarNullable(BaseElement element, string name)
        {
            variables.Add(new ElementVar(VarType.Single, element, name, true));
            return this;
        }

        internal StructElem Vars(BaseElement element, string name)
        {
            variables.Add(new ElementVar(VarType.List, element, name, false));
            return this;
        }

        public override void Print(IElementPrinter visitor, TextWriter writer)
        {
            visitor.Print(this, writer);
        }
    }

}
