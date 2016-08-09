using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        abstract class RightAssocBinExpParser<ChildParser> : BinExpParser
            where ChildParser : Parser<IExpComponent>, new()
        {
            protected virtual TokenType[] OpTokenTypes { get; }

            protected override IExpComponent ParseInner(Lexer lexer)
            {
                IExpComponent operandExp1 = Parse<IExpComponent, ChildParser>(lexer);
                if (operandExp1 == null)
                    throw new ParsingExpFailedException<ChildParser>();

                TokenType opTokenType;
                if (!lexer.ConsumeAny(out opTokenType, OpTokenTypes))
                    return operandExp1;

                var binOp = ConvertBinaryOperation(opTokenType);
                if (binOp == null)
                    throw new ParsingBinaryOperationFailedException();

                // right associativity이므로 AssignExp를 다시 가져옵니다.
                IExpComponent operandExp2 = Parse(lexer);
                if (operandExp2 == null)
                    throw new ParsingThisExpFailedException();

                return new BinaryExp(binOp.Value, operandExp1, operandExp2);
            }
        }
    }

}