using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Core.AbstractSyntax;

namespace Gum.App.Compiler
{
    public class ConstantEvaluator : IExpVisitor
    {
        // return 
        object ret;

        // variable
        CompilerContext ctx;

        public static object Visit(IExp exp, CompilerContext ctx)
        {
            var visitor = new ConstantEvaluator(ctx);
            exp.Visit(visitor);
            return visitor.ret;
        }

        public ConstantEvaluator(CompilerContext ctx)
        {
            this.ctx = ctx;
        }

        public void Visit(AssignExp exp)
        {
            throw new NotImplementedException();
        }

        public void Visit(VariableExp ve)
        {            
            if (!ctx.GetGlobal(ve.Name, out ret))
                throw new NotSupportedException();
        }

        public void Visit(IntegerExp exp)
        {
            ret = exp.Value;
        }

        public void Visit(StringExp exp)
        {
            ret = exp.Value;
        }

        public void Visit(BoolExp exp)
        {
            ret = exp.Value;
        }

        public void Visit(BinaryExp exp)
        {
            object o1 = Visit(exp.Operand1, ctx);
            object o2 = Visit(exp.Operand2, ctx);

            switch (exp.Operation)
            {
                case BinaryExpKind.Equal: // Bool: String: Integer
                    ret = o1 == o2;
                    return;

                case BinaryExpKind.NotEqual: // Bool: String: Integer
                    ret = o1 != o2;
                    return;

                case BinaryExpKind.And:   // Bool
                    ret = (bool)o1 && (bool)o2;
                    return;

                case BinaryExpKind.Or:    // Bool
                    ret = (bool)o1 || (bool)o2;
                    return;

                case BinaryExpKind.Add:   // Integer: String
                    if (o1 is int && o2 is int)
                    {
                        ret = (int)o1 + (int)o2;
                        return;
                    }

                    if (o1 is string && o2 is string)
                    {
                        ret = (string)o1 + (string)o2;
                        return;
                    }
                    throw new NotImplementedException();

                case BinaryExpKind.Sub:   // Integer
                    ret = (int)o1 - (int)o2;
                    return;

                case BinaryExpKind.Mul:   // Integer
                    ret = (int)o1 * (int)o2;
                    return;

                case BinaryExpKind.Div:   // Integer
                    ret = (int)o1 / (int)o2;
                    return;

                case BinaryExpKind.Mod:   // Integer 
                    ret = (int)o1 % (int)o2;
                    return;

                case BinaryExpKind.Less:  // Integer
                    ret = (int)o1 < (int)o2;
                    return;

                case BinaryExpKind.Greater: // Integer
                    ret = (int)o1 > (int)o2;
                    return;

                case BinaryExpKind.LessEqual: // Integer
                    ret = (int)o1 <= (int)o2;
                    return;

                case BinaryExpKind.GreaterEqual: // Integer
                    ret = (int)o1 >= (int)o2;
                    return;
            }

            throw new NotImplementedException();
        }

        public void Visit(UnaryExp exp)
        {
            object o = Visit(exp.Operand, ctx);

            switch (exp.Operation)
            {
                case UnaryExpKind.Neg: // '-' Integer
                    ret = -(int)o;
                    return;
                case UnaryExpKind.Not: // !   Bool
                    ret = !(bool)o;
                    return;
            }

            throw new NotImplementedException();
        }

        public void Visit(CallExp exp)
        {
            throw new NotImplementedException();
        }

        public void Visit(NewExp exp)
        {
            throw new NotImplementedException();
        }
        
        public void Visit(FieldExp exp)
        {
            throw new NotImplementedException();
        }
    }
}
