using System;
using Gum.Lang.AbstractSyntax;
using System.Collections.Generic;
using System.Linq;

namespace Gum.Translator.Text2AST.Parsing
{
    // constant
    // 1. x.y, f(x), a[x], x++, x--, new, 
    //    typeof default checked unchecked delegate?
    class PrimaryExpParser : Parser<IExpComponent>
    {
        IExpComponent ParseSingleExp(Lexer lexer)
        {
            /*IExpComponent result = Parse<IExpComponent, NewExpParser>(lexer);
            if (lexer != null)
                return result;*/

            string intValue;
            if( lexer.Consume(TokenType.IntValue, out intValue))
                return new IntegerExp(int.Parse(intValue));

            IDExp idExp = Parser<IDExp, IDExpParser>.Parse(lexer);
            if (idExp != null)
                return idExp;

            if (lexer.Consume(TokenType.TrueValue))
                return new BoolExp(true);

            if (lexer.Consume(TokenType.FalseValue))
                return new BoolExp(false);

            string value;
            if (lexer.Consume(TokenType.StringValue, out value))
                return new StringExp(value);

            return null;
        }

        private CallExp ParseCallExp(IExpComponent leftExp, Lexer lexer)
        {
            if (!lexer.Consume(TokenType.LParen))
                return null;

            var args = new List<IExpComponent>();
            while (!lexer.Consume(TokenType.RParen))
            {
                if (args.Count != 0 && !lexer.Consume(TokenType.Comma))
                    throw new ParsingExpTokenFailedException<CallExp>(TokenType.Comma);

                IExpComponent exp = Parse<IExpComponent, ExpComponentParser>(lexer);
                if (exp == null)
                    throw new ParsingExpFailedException<CallExp,IExpComponent>();

                args.Add(exp);
            }

            return new CallExp(leftExp, args);
        }

        private MemberExp ParseMemberExp(IExpComponent leftExp, Lexer lexer)
        {   
            string memberID;
            if (!lexer.Consume(TokenType.Identifier, out memberID))
                throw new ParsingExpTokenFailedException<MemberExp>(TokenType.Identifier);

            // comma-separated
            var typeIDs = new List<TypeID>();
            if (lexer.Consume(TokenType.Less))
            {
                var typeID = Parse<TypeID, TypeIDParser>(lexer);
                if (typeID == null)
                    throw new ParsingExpFailedException<MemberExp, TypeID>();
                typeIDs.Add(typeID);

                while (lexer.Consume(TokenType.Comma))
                {
                    typeID = Parse<TypeID, TypeIDParser>(lexer);
                    if (typeID == null)
                        throw new ParsingExpFailedException<MemberExp, TypeID>();
                    typeIDs.Add(typeID);
                }

                if (!lexer.Consume(TokenType.Greater))
                    throw new ParsingExpTokenFailedException<MemberExp>(TokenType.Greater);
            }

            return new MemberExp(leftExp, memberID, typeIDs);
        }

        protected override IExpComponent ParseInner(Lexer lexer)
        {
            IExpComponent leftExp = ParseSingleExp(lexer);
            if (leftExp == null)
                throw new ParsingPrimarySingleExpFailed();

            while(true)
            {
                // . < ( [ ++ -- 
                if( lexer.Consume(TokenType.PlusPlus) )
                {
                    leftExp = new UnaryExp(UnaryExpKind.PostfixInc, leftExp);
                    continue;
                }

                if (lexer.Consume(TokenType.MinusMinus))
                {
                    leftExp = new UnaryExp(UnaryExpKind.PostfixDec, leftExp);
                    continue;
                }

                if (lexer.Consume(TokenType.Dot))
                {


                    leftExp = ParseMemberExp(leftExp, lexer);
                    continue;                    
                }

                if( lexer.Consume(TokenType.LBracket))
                {
                    IExpComponent indexExp = Parse<IExpComponent, ExpComponentParser>(lexer);
                    if (indexExp == null)
                        throw new ParsingExpFailedException<ArrayExp,IExpComponent>();

                    if (!lexer.Consume(TokenType.RBracket))
                        throw new ParsingExpTokenFailedException<ArrayExp>(TokenType.RBracket);

                    leftExp = new ArrayExp(leftExp, indexExp);
                    continue;
                }

                // call
                IExpComponent callExp = ParseCallExp(leftExp, lexer);
                if (callExp != null)
                {
                    leftExp = callExp;
                    continue;
                }

                break;
            }

            return leftExp;
        }

    }
}