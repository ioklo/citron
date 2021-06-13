using Gum.LexicalAnalysis;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.ParserMisc;

namespace Gum
{
    using ExpParseResult = ParseResult<Exp>;
    using StringExpParseResult = ParseResult<StringExp>;

    class ExpParser
    {
        Parser parser; // parentComponent
        Lexer lexer;

        public ExpParser(Parser parser, Lexer lexer)
        {
            this.parser = parser;
            this.lexer = lexer;
        }

        async ValueTask<ExpParseResult> ParseLeftAssocBinaryOpExpAsync(
            ParserContext context,
            Func<ParserContext, ValueTask<ExpParseResult>> ParseBaseExpAsync,
            (Token Token, BinaryOpKind OpKind)[] infos)
        {            
            if (!Parse(await ParseBaseExpAsync(context), ref context, out var exp0))
                return ExpParseResult.Invalid;

            while (true)
            {
                BinaryOpKind? opKind = null;

                var lexResult = await lexer.LexNormalModeAsync(context.LexerContext, true);
                if (lexResult.HasValue)
                {
                    foreach (var info in infos)
                    {
                        if (info.Token == lexResult.Token)
                        {
                            opKind = info.OpKind;
                            context = context.Update(lexResult.Context);
                            break;
                        }
                    }
                }

                if (!opKind.HasValue)
                    return new ExpParseResult(exp0, context);

                if (!Parse(await ParseBaseExpAsync(context), ref context, out var exp1))
                    return ExpParseResult.Invalid;

                // Fold
                exp0 = new BinaryOpExp(opKind.Value, exp0, exp1);
            }
        }

        Exp? HandleUnaryMinusWithIntLiteral(UnaryOpKind kind, Exp exp)
        {
            if( kind == UnaryOpKind.Minus && exp is IntLiteralExp intLiteralExp)
            {
                return new IntLiteralExp(-intLiteralExp.Value);
            }

            return null;
        }

        #region Single
        async ValueTask<ExpParseResult> ParseSingleExpAsync(ParserContext context)
        {
            var newExpResult = await ParseNewExpAsync(context);
            if (newExpResult.HasValue)
                return newExpResult;

            var refExpResult = await ParseRefExpAsync(context);
            if (refExpResult.HasValue)
                return refExpResult;

            var parenExpResult = await ParseParenExpAsync(context);
            if (parenExpResult.HasValue)
                return parenExpResult;            

            var boolExpResult = await ParseBoolLiteralExpAsync(context);
            if (boolExpResult.HasValue)
                return new ExpParseResult(boolExpResult.Elem, boolExpResult.Context);

            var intExpResult = await ParseIntLiteralExpAsync(context);
            if (intExpResult.HasValue)
                return new ExpParseResult(intExpResult.Elem, intExpResult.Context);

            var stringExpResult = await ParseStringExpAsync(context);
            if (stringExpResult.HasValue)
                return new ExpParseResult(stringExpResult.Elem, stringExpResult.Context);

            var listExpResult = await ParseListExpAsync(context);
            if (listExpResult.HasValue)
                return new ExpParseResult(listExpResult.Elem, listExpResult.Context);

            var idExpResult = await ParseIdentifierExpAsync(context);
            if (idExpResult.HasValue)
                return idExpResult;

            return ExpParseResult.Invalid;
        }

        #endregion
        
        #region Primary, Postfix Inc/Dec       

        async ValueTask<ParseResult<Argument>> ParseArgumentAsync(ParserContext context)
        {
            // params, ref
            if (Accept<ParamsToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (!Parse(await ParseExpAsync(context), ref context, out var exp))
                    return ParseResult<Argument>.Invalid;

                return new ParseResult<Argument>(new Argument.Params(exp), context);
            }
            else if (Accept<RefToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (!Parse(await ParseExpAsync(context), ref context, out var exp))
                    return ParseResult<Argument>.Invalid;

                return new ParseResult<Argument>(new Argument.Ref(exp), context);
            }
            else
            {
                if (!Parse(await ParseExpAsync(context), ref context, out var exp))
                    return ParseResult<Argument>.Invalid;

                return new ParseResult<Argument>(new Argument.Normal(exp), context);
            }
        }

        async ValueTask<ParseResult<ImmutableArray<Argument>>> ParseCallArgs(ParserContext context)
        {
            if (!Accept<LParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<ImmutableArray<Argument>>.Invalid;
            
            var builder = ImmutableArray.CreateBuilder<Argument>();
            while (!Accept<RParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (0 < builder.Count)
                    if (!Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                        return ParseResult<ImmutableArray<Argument>>.Invalid;

                if (!Parse(await ParseArgumentAsync(context), ref context, out var arg))
                    return ParseResult<ImmutableArray<Argument>>.Invalid;

                builder.Add(arg);
            }

            return new ParseResult<ImmutableArray<Argument>>(builder.ToImmutable(), context);
        }

        static (Token Token, UnaryOpKind OpKind)[] primaryInfos = new (Token Token, UnaryOpKind OpKind)[]
        {
            (PlusPlusToken.Instance, UnaryOpKind.PostfixInc),
            (MinusMinusToken.Instance, UnaryOpKind.PostfixDec),
        };

        // postfix
        internal async ValueTask<ExpParseResult> ParsePrimaryExpAsync(ParserContext context)
        {
            ValueTask<ExpParseResult> ParseBaseExpAsync(ParserContext context) => ParseSingleExpAsync(context);

            if (!Parse(await ParseBaseExpAsync(context), ref context, out var exp))
                return ExpParseResult.Invalid;

            while (true)
            {
                // Unary일수도 있고, ()일수도 있다
                var lexResult = await lexer.LexNormalModeAsync(context.LexerContext, true);
                if (!lexResult.HasValue) break;

                (Token Token, UnaryOpKind OpKind)? primaryInfo = null;
                foreach (var info in primaryInfos)
                    if (info.Token == lexResult.Token)
                    {
                        // TODO: postfix++이 두번 이상 나타나지 않도록 한다
                        primaryInfo = info;
                        break;
                    }

                if (primaryInfo.HasValue)
                {
                    context = context.Update(lexResult.Context);

                    // Fold
                    exp = new UnaryOpExp(primaryInfo.Value.OpKind, exp);
                    continue;
                }

                // [ ... ]
                if (Accept<LBracketToken>(lexResult, ref context))
                {
                    if (!Parse(await ParseExpAsync(context), ref context, out var index))
                        return ExpParseResult.Invalid;

                    if (!Accept<RBracketToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                        return ExpParseResult.Invalid;

                    exp = new IndexerExp(exp, index);
                    continue;
                }

                // . id < >
                if (Accept<DotToken>(lexResult, ref context))
                {
                    if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var idToken))
                        return ExpParseResult.Invalid;

                    // <
                    Parse(await parser.ParseTypeArgs(context), ref context, out var typeArgs);

                    exp = new MemberExp(exp, idToken.Value, typeArgs);
                    continue;                    
                }

                // (..., ... )                
                if (Parse(await ParseCallArgs(context), ref context, out var callArgs))
                {                    
                    exp = new CallExp(exp, callArgs);
                    continue;
                }

                break;
            }

            return new ExpParseResult(exp, context);
        }
        #endregion

        #region Unary, Prefix Inc/Dec
        static (Token Token, UnaryOpKind OpKind)[] unaryInfos = new (Token Token, UnaryOpKind OpKind)[]
        {
            (MinusToken.Instance, UnaryOpKind.Minus),
            (ExclToken.Instance, UnaryOpKind.LogicalNot),
            (PlusPlusToken.Instance, UnaryOpKind.PrefixInc),
            (MinusMinusToken.Instance, UnaryOpKind.PrefixDec),
        };

        async ValueTask<ExpParseResult> ParseUnaryExpAsync(ParserContext context)
        {
            ValueTask<ExpParseResult> ParseBaseExpAsync(ParserContext context) => ParsePrimaryExpAsync(context);

            UnaryOpKind? opKind = null;

            var lexResult = await lexer.LexNormalModeAsync(context.LexerContext, true);
            if (lexResult.HasValue)
            {
                foreach (var info in unaryInfos)
                {
                    if (info.Token == lexResult.Token)
                    {
                        opKind = info.OpKind;
                        context = context.Update(lexResult.Context);
                        break;
                    }
                }
            }

            if (opKind.HasValue)
            {
                if (!Parse(await ParseUnaryExpAsync(context), ref context, out var exp))
                    return ExpParseResult.Invalid;

                var handledExp = HandleUnaryMinusWithIntLiteral(opKind.Value, exp);
                if (handledExp != null)                
                    return new ExpParseResult(handledExp, context);

                return new ExpParseResult(new UnaryOpExp(opKind.Value, exp), context);
            }
            else
            {
                return await ParseBaseExpAsync(context);
            }
        }
        #endregion

        #region Multiplicative, LeftAssoc
        static (Token Token, BinaryOpKind OpKind)[] multiplicativeInfos = new (Token Token, BinaryOpKind OpKind)[]
        {
            (StarToken.Instance, BinaryOpKind.Multiply),
            (SlashToken.Instance, BinaryOpKind.Divide),
            (PercentToken.Instance, BinaryOpKind.Modulo),
        };

        ValueTask<ExpParseResult> ParseMultiplicativeExpAsync(ParserContext context)
        {
            return ParseLeftAssocBinaryOpExpAsync(context, ParseUnaryExpAsync, multiplicativeInfos);
        }
        #endregion


        #region Additive, LeftAssoc
        static (Token Token, BinaryOpKind OpKind)[] additiveInfos = new (Token Token, BinaryOpKind OpKind)[]
        {
            (PlusToken.Instance, BinaryOpKind.Add),
            (MinusToken.Instance, BinaryOpKind.Subtract),
        };

        ValueTask<ExpParseResult> ParseAdditiveExpAsync(ParserContext context)
        {
            return ParseLeftAssocBinaryOpExpAsync(context, ParseMultiplicativeExpAsync, additiveInfos);
        }
        #endregion

        #region Test, LeftAssoc
        static (Token Token, BinaryOpKind OpKind)[] testInfos = new (Token Token, BinaryOpKind OpKind)[]
        {
            (GreaterThanEqualToken.Instance, BinaryOpKind.GreaterThanOrEqual),
            (LessThanEqualToken.Instance, BinaryOpKind.LessThanOrEqual),
            (LessThanToken.Instance, BinaryOpKind.LessThan),
            (GreaterThanToken.Instance, BinaryOpKind.GreaterThan),
        };

        ValueTask<ExpParseResult> ParseTestExpAsync(ParserContext context)
        {
            return ParseLeftAssocBinaryOpExpAsync(context, ParseAdditiveExpAsync, testInfos);
        }
        #endregion

        #region Equality, Left Assoc
        static (Token Token, BinaryOpKind OpKind)[] equalityInfos = new (Token Token, BinaryOpKind OpKind)[]
        {
            (EqualEqualToken.Instance, BinaryOpKind.Equal),
            (ExclEqualToken.Instance, BinaryOpKind.NotEqual),
        };

        ValueTask<ExpParseResult> ParseEqualityExpAsync(ParserContext context)
        {
            return ParseLeftAssocBinaryOpExpAsync(context, ParseTestExpAsync, equalityInfos);
        }
        #endregion
        

        #region Assignment, Right Assoc
        async ValueTask<ExpParseResult> ParseAssignExpAsync(ParserContext context)
        {
            ValueTask<ExpParseResult> ParseBaseExpAsync(ParserContext context) => ParseEqualityExpAsync(context);

            if (!Parse(await ParseBaseExpAsync(context), ref context, out var exp0))
                return ExpParseResult.Invalid;            

            if (!Accept<EqualToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return new ExpParseResult(exp0, context);

            if (!Parse(await ParseAssignExpAsync(context), ref context, out var exp1))            
                return ExpParseResult.Invalid;

            return new ExpParseResult(new BinaryOpExp(BinaryOpKind.Assign, exp0, exp1), context);
        }

        #endregion

        #region LambdaExpression, Right Assoc
        async ValueTask<ExpParseResult> ParseLambdaExpAsync(ParserContext context)
        {
            var initialContext = context;
            var paramsBuilder = ImmutableArray.CreateBuilder<LambdaExpParam>();

            // (), (a, b)
            // (int a)
            // a            
            if (Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var idToken))
            {
                paramsBuilder.Add(new LambdaExpParam(null, idToken.Value));
            }
            else if (Accept<LParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                while(!Accept<RParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                {
                    if (0 < paramsBuilder.Count)
                        if (!Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                            return await Invalid();

                    // id id or id
                    if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var firstIdToken))
                        return await Invalid();

                    if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var secondIdToken))                    
                        paramsBuilder.Add(new LambdaExpParam(null, firstIdToken.Value));
                    else
                        paramsBuilder.Add(new LambdaExpParam(new IdTypeExp(firstIdToken.Value, default), secondIdToken.Value));
                }
            }

            // =>
            if (!Accept<EqualGreaterThanToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                return await Invalid();
            }

            // exp => return exp;
            // { ... }
            Stmt body;
            if (Peek<LBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true)))
            {
                if (!Parse(await parser.ParseStmtAsync(context), ref context, out var stmtBody))
                    return await Invalid();

                body = stmtBody;
            }
            else
            {
                if (!Parse(await parser.ParseExpAsync(context), ref context, out var expBody))
                    return await Invalid();

                body = new ReturnStmt(expBody);
            }

            return new ExpParseResult(new LambdaExp(paramsBuilder.ToImmutable(), body), context);

            ValueTask<ExpParseResult> Invalid() => ParseAssignExpAsync(initialContext); // 초기 상태로 되돌린 다음 Assign 해보기
        }
        #endregion

        public ValueTask<ExpParseResult> ParseExpAsync(ParserContext context)
        {
            return ParseLambdaExpAsync(context);
        }

        async ValueTask<ExpParseResult> ParseNewExpAsync(ParserContext context)
        {
            // <NEW> <TYPEEXP> <LPAREN> CallArgs <RPAREN>
            if (!Accept<NewToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ExpParseResult.Invalid;

            if (!Parse(await parser.ParseTypeExpAsync(context), ref context, out var type))
                return ExpParseResult.Invalid;

            if (!Parse(await ParseCallArgs(context), ref context, out var callArgs))
                return ExpParseResult.Invalid;

            return new ExpParseResult(new NewExp(type, callArgs), context);
        }

        async ValueTask<ExpParseResult> ParseRefExpAsync(ParserContext context)
        {
            // <REF> Exp
            if (!Accept<RefToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ExpParseResult.Invalid;

            if (!Parse(await ParseExpAsync(context), ref context, out var exp))
                return ExpParseResult.Invalid;

            return new ExpParseResult(new RefExp(exp), context);
        }

        async ValueTask<ExpParseResult> ParseParenExpAsync(ParserContext context)
        {
            if (!Accept<LParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ExpParseResult.Invalid;
            
            if (!Parse(await ParseExpAsync(context), ref context, out var exp))
                return ExpParseResult.Invalid;

            if (!Accept<RParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ExpParseResult.Invalid;

            return new ExpParseResult(exp, context);
        }

        async ValueTask<ExpParseResult> ParseBoolLiteralExpAsync(ParserContext context)
        {
            if (Accept<BoolToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var boolToken))
                return new ExpParseResult(new BoolLiteralExp(boolToken.Value), context);

            return ExpParseResult.Invalid;
        }

        async ValueTask<ExpParseResult> ParseIntLiteralExpAsync(ParserContext context)
        {            
            if (Accept<IntToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var intToken))
                return new ExpParseResult(new IntLiteralExp(intToken.Value), context);

            return ExpParseResult.Invalid;
        }

        // 스트링 파싱
        public async ValueTask<StringExpParseResult> ParseStringExpAsync(ParserContext context)
        {
            if (!Accept<DoubleQuoteToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return StringExpParseResult.Invalid;

            var builder = ImmutableArray.CreateBuilder<StringExpElement>();
            while (!Accept<DoubleQuoteToken>(await lexer.LexStringModeAsync(context.LexerContext), ref context))
            {
                if (Accept<TextToken>(await lexer.LexStringModeAsync(context.LexerContext), ref context, out var textToken))
                {
                    builder.Add(new TextStringExpElement(textToken.Text));
                    continue;
                }
                
                if (Accept<IdentifierToken>(await lexer.LexStringModeAsync(context.LexerContext), ref context, out var idToken))
                {
                    builder.Add(new ExpStringExpElement(new IdentifierExp(idToken.Value, default)));
                    continue;
                }

                // ${
                if (Accept<DollarLBraceToken>(await lexer.LexStringModeAsync(context.LexerContext), ref context))
                {
                    // TODO: EndInnerExpToken 일때 빠져나와야 한다는 표시를 해줘야 한다
                    if (!Parse(await ParseExpAsync(context), ref context, out var exp))
                        return StringExpParseResult.Invalid;

                    if (!Accept<RBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                        return StringExpParseResult.Invalid;

                    builder.Add(new ExpStringExpElement(exp));
                    continue;
                }

                // 나머지는 에러
                return StringExpParseResult.Invalid;
            }

            return new StringExpParseResult(new StringExp(builder.ToImmutable()), context);
        }

        public async ValueTask<ExpParseResult> ParseListExpAsync(ParserContext context)
        {
            if (!Accept<LBracketToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ExpParseResult.Invalid;

            var builder = ImmutableArray.CreateBuilder<Exp>();
            while (!Accept<RBracketToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (0 < builder.Count)
                    if (!Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                        return ExpParseResult.Invalid;

                if (!Parse(await ParseExpAsync(context), ref context, out var elem))
                    return ExpParseResult.Invalid;

                builder.Add(elem);
            }

            return new ExpParseResult(new ListExp(null, builder.ToImmutable()), context);
        }

        async ValueTask<ExpParseResult> ParseIdentifierExpAsync(ParserContext context)
        {   
            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var idToken))
                return ExpParseResult.Invalid;

            // 실패해도 괜찮다            
            Parse(await Try(parser.ParseTypeArgs, context), ref context, out var typeArgs);

            return new ExpParseResult(new IdentifierExp(idToken.Value, typeArgs), context);
        }
    }
}
