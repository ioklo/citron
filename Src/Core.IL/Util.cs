using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL
{
    public static class Util
    {
        public static bool IsBinary(this OperatorKind kind)
        {
            switch (kind)
            {
                case OperatorKind.Equal: // Bool: String: Integer
                case OperatorKind.NotEqual: // Bool: String: Integer
                case OperatorKind.And:   // Bool
                case OperatorKind.Or:    // Bool
                case OperatorKind.Xor:   // Bool

                case OperatorKind.Add:   // Integer: String
                case OperatorKind.Sub:   // Integer
                case OperatorKind.Mul:   // Integer
                case OperatorKind.Div:   // Integer
                case OperatorKind.Mod:   // Integer 
                case OperatorKind.Exp:   // Integer
                case OperatorKind.Less:  // Integer
                case OperatorKind.Greater: // Integer
                case OperatorKind.LessEqual: // Integer
                case OperatorKind.GreaterEqual: // Integer
                    return true;
            }

            return false;
        }

        public static bool IsUnary(this OperatorKind kind)
        {
            switch (kind)
            {
                case OperatorKind.Neg: // '-' Integer
                case OperatorKind.Not: // !   Bool
                    return true;
            }

            return false;
        }

        public static int ArgCount(this OperatorKind kind)
        {
            if (kind.IsBinary())
                return 2;
            else if (kind.IsUnary())
                return 1;

            throw new NotImplementedException();
        }

        public static int RetValCount(this OperatorKind kind)
        {
            return 1;
        }
        
    }
}
