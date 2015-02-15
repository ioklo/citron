using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.App.Compiler.AST;

namespace Gum.App.Compiler
{
    class ConstantEvaluator : IExpVisitor<object>
    {
        CompilerContext ctx;

        public ConstantEvaluator(CompilerContext ctx)
        {
            this.ctx = ctx;
        }

        public object Visit(AssignExp exp)
        {
            throw new NotImplementedException();
        }

        public object Visit(VariableExp ve)
        {
            if (ve.Offsets.Count != 0)
                throw new NotSupportedException();

            object v;
            if (!ctx.GetGlobal(ve.Name, out v))
                throw new NotSupportedException();

            return v;
        }

        public object Visit(IntegerExp exp)
        {
            return exp.Value;
        }

        public object Visit(StringExp exp)
        {
            return exp.Value;
        }

        public object Visit(BoolExp exp)
        {
            return exp.Value;
        }

        public object Visit(BinaryExp exp)
        {
            object o1 = exp.Operand1.Visit(this);
            object o2 = exp.Operand2.Visit(this);

            switch (exp.Operation)
            {
                case BinaryExpKind.Equal: // Bool: String: Integer
                    return o1 == o2;

                case BinaryExpKind.NotEqual: // Bool: String: Integer
                    return o1 != o2;

                case BinaryExpKind.And:   // Bool
                    return (bool)o1 && (bool)o2;

                case BinaryExpKind.Or:    // Bool
                    return (bool)o1 || (bool)o2;

                case BinaryExpKind.Add:   // Integer: String
                    if (o1 is int && o2 is int)
                        return (int)o1 + (int)o2;
                    if (o1 is string && o2 is string)
                        return (string)o1 + (string)o2;
                    throw new NotImplementedException();

                case BinaryExpKind.Sub:   // Integer
                    return (int)o1 - (int)o2;

                case BinaryExpKind.Mul:   // Integer
                    return (int)o1 * (int)o2;

                case BinaryExpKind.Div:   // Integer
                    return (int)o1 / (int)o2;

                case BinaryExpKind.Mod:   // Integer 
                    return (int)o1 % (int)o2;

                case BinaryExpKind.Less:  // Integer
                    return (int)o1 < (int)o2;

                case BinaryExpKind.Greater: // Integer
                    return (int)o1 > (int)o2;

                case BinaryExpKind.LessEqual: // Integer
                    return (int)o1 <= (int)o2;

                case BinaryExpKind.GreaterEqual: // Integer
                    return (int)o1 >= (int)o2;
            }

            throw new NotImplementedException();
        }

        public object Visit(UnaryExp exp)
        {
            object o = exp.Operand.Visit(this);

            switch (exp.Operation)
            {
                case UnaryExpKind.Neg: // '-' Integer
                    return -(int)o;
                case UnaryExpKind.Not: // !   Bool
                    return !(bool)o;
            }

            throw new NotImplementedException();
        }

        public object Visit(CallExp exp)
        {
            throw new NotImplementedException();
        }

        public object Visit(NewExp exp)
        {
            throw new NotImplementedException();
        }
    }
}
