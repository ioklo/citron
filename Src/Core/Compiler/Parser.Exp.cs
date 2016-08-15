using System;
using Gum.Data.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        UnaryExpKind ConvertPreUnaryOperation(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.PlusPlus: return UnaryExpKind.PrefixInc;
                case TokenType.MinusMinus: return UnaryExpKind.PrefixDec;
                case TokenType.Plus: return UnaryExpKind.Plus;
                case TokenType.Minus: return UnaryExpKind.Minus;
                case TokenType.Exclamation: return UnaryExpKind.Not;
                case TokenType.Tilde: return UnaryExpKind.Neg;
            }

            throw CreateException();
        }

        private BinaryExpKind ConvertBinaryOperation(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.AmperAmper: return BinaryExpKind.ConditionalAnd;
                case TokenType.BarBar: return BinaryExpKind.ConditionalOr;

                case TokenType.EqualEqual: return BinaryExpKind.Equal;
                case TokenType.NotEqual: return BinaryExpKind.NotEqual;

                case TokenType.Less: return BinaryExpKind.Less;
                case TokenType.LessEqual: return BinaryExpKind.LessEqual;
                case TokenType.Greater: return BinaryExpKind.Greater;
                case TokenType.GreaterEqual: return BinaryExpKind.GreaterEqual;
                case TokenType.Plus: return BinaryExpKind.Add;
                case TokenType.Minus: return BinaryExpKind.Sub;
                case TokenType.Star: return BinaryExpKind.Mul;
                case TokenType.Slash: return BinaryExpKind.Div;
                case TokenType.Percent: return BinaryExpKind.Mod;

                case TokenType.Equal: return BinaryExpKind.Assign;
                case TokenType.LessLess: return BinaryExpKind.ShiftLeft;
                case TokenType.GreaterGreater: return BinaryExpKind.ShiftRight;

            }

            throw CreateException();
        }

        private IExpComponent ParseLeftAssocBinExp(Func<IExpComponent> ParseChild, params TokenType[] OpTokenTypes)
        {
            IExpComponent leftExp = ParseChild();

            TokenType opTokenType;
            while (ConsumeAny(out opTokenType, OpTokenTypes))
            {
                IExpComponent rightExp = ParseChild();

                var binOp = ConvertBinaryOperation(opTokenType);
                leftExp = new BinaryExp(binOp, leftExp, rightExp);
            }

            return leftExp;
        }

        private IExpComponent ParseRightAssocBinExp(Func<IExpComponent> ParseChild, params TokenType[] OpTokenTypes)
        {
            IExpComponent operandExp1 = ParseChild();

            TokenType opTokenType;
            if (!ConsumeAny(out opTokenType, OpTokenTypes))
                return operandExp1;

            var binOp = ConvertBinaryOperation(opTokenType);

            // right associativity이므로 AssignExp를 다시 가져옵니다.
            IExpComponent operandExp2 = ParseExp();

            return new BinaryExp(binOp, operandExp1, operandExp2);
        }

        private IExpComponent ParseSingleExp()
        {
            /*IExpComponent result = Parse<IExpComponent, NewExpParser>(lexer);
            if (lexer != null)
                return result;*/

            string intValue;
            if (Consume(TokenType.IntValue, out intValue))
                return new IntegerExp(int.Parse(intValue));

            IDExp idExp;
            if (RollbackIfFailed(out idExp, ParseIDExp))
                return idExp;

            if (Consume(TokenType.TrueValue))
                return new BoolExp(true);

            if (Consume(TokenType.FalseValue))
                return new BoolExp(false);

            string value;
            if (Consume(TokenType.StringValue, out value))
                return new StringExp(value);

            throw CreateException();
        }

        private CallExp ParseCallExp(IExpComponent leftExp)
        {
            ConsumeOrThrow(TokenType.LParen);

            var args = new List<IExpComponent>();
            while (!Consume(TokenType.RParen))
            {
                if (args.Count != 0)
                    ConsumeOrThrow(TokenType.Comma);

                IExpComponent exp = ParseExp();
                args.Add(exp);
            }

            return new CallExp(leftExp, args);
        }

        private MemberExp ParseMemberExp(IExpComponent leftExp)
        {
            string memberID;
            ConsumeOrThrow(TokenType.Identifier, out memberID);

            // comma-separated
            var typeIDs = new List<TypeID>();
            if (Consume(TokenType.Less))
            {
                var typeID = ParseTypeID();
                typeIDs.Add(typeID);

                while (Consume(TokenType.Comma))
                {
                    typeID = ParseTypeID();
                    typeIDs.Add(typeID);
                }

                ConsumeOrThrow(TokenType.Greater);
            }

            return new MemberExp(leftExp, memberID, typeIDs);
        }

        

        // constant
        // 1. x.y, f(x), a[x], x++, x--, new, 
        //    typeof default checked unchecked delegate?
        private IExpComponent ParsePrimaryExp()
        {
            IExpComponent leftExp = ParseSingleExp();

            while (true)
            {
                // . < ( [ ++ -- 
                if (Consume(TokenType.PlusPlus))
                {
                    leftExp = new UnaryExp(UnaryExpKind.PostfixInc, leftExp);
                    continue;
                }

                if (Consume(TokenType.MinusMinus))
                {
                    leftExp = new UnaryExp(UnaryExpKind.PostfixDec, leftExp);
                    continue;
                }

                if (Consume(TokenType.Dot))
                {
                    leftExp = ParseMemberExp(leftExp);
                    continue;
                }

                if (Consume(TokenType.LBracket))
                {
                    IExpComponent indexExp = ParseExp();
                    ConsumeOrThrow(TokenType.RBracket);
                    leftExp = new ArrayExp(leftExp, indexExp);
                    continue;
                }

                // call
                IExpComponent callExp;
                if (RollbackIfFailed(out callExp, () => ParseCallExp(leftExp)))
                {
                    leftExp = callExp;
                    continue;
                }

                break;
            }

            return leftExp;
        }



        // 2. + - ! ~ ++x --x 
        // TODO: (T)x
        private IExpComponent ParseUnaryExp()
        {
            TokenType tokenType;
            if (!ConsumeAny(out tokenType,
                    TokenType.Plus, TokenType.Minus,
                    TokenType.Exclamation, TokenType.Tilde, TokenType.PlusPlus, TokenType.MinusMinus))
            {
                return ParsePrimaryExp();
            }

            var unaryExpKind = ConvertPreUnaryOperation(tokenType);

            IExpComponent operandExp = ParsePrimaryExp();

            return new UnaryExp(unaryExpKind, operandExp);
        }

        // 3. * / %
        private IExpComponent ParserMultiplicativeExp()
        {
            return ParseLeftAssocBinExp(ParseUnaryExp, TokenType.Star, TokenType.Slash, TokenType.Percent);
        }

        // 4. + - 
        private IExpComponent ParseAdditiveExp()
        {
            return ParseLeftAssocBinExp(ParserMultiplicativeExp, TokenType.Plus, TokenType.Minus);
        }

        // 5. << >>
        private IExpComponent ParseShiftExp()
        {
            return ParseLeftAssocBinExp(ParseAdditiveExp, TokenType.LessLess, TokenType.GreaterGreater);

        }


        // 6. Relational and type testing 
        // < > <= >=  // is as
        private IExpComponent ParseRelationalAndTypeTestingExp()
        {
            return ParseLeftAssocBinExp(ParseShiftExp,
                TokenType.Less, TokenType.Greater,
                TokenType.LessEqual, TokenType.GreaterEqual);
        }

        // 7. == !=
        private IExpComponent ParseEqualityExp()
        {
            return ParseLeftAssocBinExp(ParseRelationalAndTypeTestingExp, TokenType.EqualEqual, TokenType.NotEqual);
        }

        // 8. &
        private IExpComponent ParseLogicalANDExp()
        {
            return ParseLeftAssocBinExp(ParseEqualityExp, TokenType.Amper);
        }

        // 9. ^
        private IExpComponent ParseLogicalXORExp()
        {
            return ParseLeftAssocBinExp(ParseLogicalANDExp, TokenType.Caret);
        }

        // 10. |
        private IExpComponent ParseLogicalORExp()
        {
            return ParseLeftAssocBinExp(ParseLogicalXORExp, TokenType.Bar);
        }

        // 11. &&
        private IExpComponent ParseConditionalANDExp()
        {
            return ParseLeftAssocBinExp(ParseLogicalORExp, TokenType.AmperAmper);
        }

        // 12. ||, left associativity
        private IExpComponent ParseConditionalORExp()
        {
            return ParseLeftAssocBinExp(ParseConditionalANDExp, TokenType.BarBar);
        }

        // 15. Assignment and (lambda expression '=>', not supported), right associative
        // = *= /= %= += -= <<= >>= &= ^= |= 

        static private TokenType[] assignExptokenTypes = new[]
        {
            TokenType.Equal, TokenType.StarEqual, TokenType.SlashEqual, TokenType.PercentEqual,
            TokenType.PlusEqual, TokenType.MinusEqual, TokenType.LessLessEqual, TokenType.GreaterGreaterEqual,
            TokenType.AmperEqual, TokenType.CaretEqual, TokenType.BarEqual
        };

        private IExpComponent ParseAssignExp()
        {
            return ParseRightAssocBinExp(ParseConditionalORExp, assignExptokenTypes);
        }

        private IExpComponent ParseExp()
        {
            return ParseAssignExp();
        }
    }
}