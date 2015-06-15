using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public interface IExpVisitor
    {
        void Visit(AssignExp exp);
        void Visit(VariableExp exp);
        void Visit(IntegerExp exp);
        void Visit(StringExp exp);
        void Visit(BoolExp exp);
        void Visit(BinaryExp exp);
        void Visit(UnaryExp exp);
        void Visit(CallExp exp);
        void Visit(NewExp exp);
        void Visit(FieldExp exp);
    }
}
