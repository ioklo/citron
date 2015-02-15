using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.AST
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
    }

    public interface IExpVisitor<Result>
    {
        Result Visit(AssignExp exp);
        Result Visit(VariableExp exp);
        Result Visit(IntegerExp exp);
        Result Visit(StringExp exp);
        Result Visit(BoolExp exp);
        Result Visit(BinaryExp exp);
        Result Visit(UnaryExp exp);
        Result Visit(CallExp exp);
        Result Visit(NewExp exp);
    }
}
