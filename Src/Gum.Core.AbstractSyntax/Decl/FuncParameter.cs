using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gum.Core.AbstractSyntax;

namespace Gum.Core.AbstractSyntax
{
    public class FuncParameter
    {
        public FuncParamModifier Modifier {get; private set;}
        public TypeIdentifier Type { get; private set; }
        public VarIdentifier Name { get; private set; }    

        public FuncParameter(FuncParamModifier paramModifier, TypeIdentifier type, VarIdentifier name)
        {
            Modifier = paramModifier;
            Type = type;
            Name = name;
        }
    }
}
