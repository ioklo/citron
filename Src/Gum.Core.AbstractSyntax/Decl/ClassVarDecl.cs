using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.AbstractSyntax
{
    public class ClassVarDecl : VarDecl
    {
        public AccessModifier AccessModifier { get; private set; }

        public ClassVarDecl(TypeIdentifier typeID, IEnumerable<NameAndExp> nameAndExps, AccessModifier accessModifier)
            : base(typeID, nameAndExps)
        {
            AccessModifier = accessModifier;
        }
    }
}
