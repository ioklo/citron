using Gum.Core.AbstractSyntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.TypedAST
{
    // Gum.Structure.TypedAST

    // AbstractSyntax -> TypedAST
    internal class TypedBinaryExp 
    {
        public BinaryExpKind Operation { get; private set; }
        public ITypedExp Operand1 { get; private set; }
        public ITypedExp Operand2 { get; private set; }
    }

    // static interface?

    // 각각의 일들이 있고, 이걸 바인딩 시켜주는 부분은 다른 곳에 있어야 한다
    internal static class Translator
    {
        // FileUnit -> TypedFileUnit
        public static TypedFileUnit Translate(FileUnit fileUnit)
        {
            return null;
        }

        public static TypedExpStmt Translate(ExpStmt expStmt)
        {
        }

        public static TypedBinaryExp Translate(BinaryExp binExp)
        {
            var typedExp1 = Translate(binExp.Operand1);
            var typedExp2 = Translate(binExp.Operand2);

            new TypedBinaryExp(typedExp1, typedExp2, )

        }
    }
}
