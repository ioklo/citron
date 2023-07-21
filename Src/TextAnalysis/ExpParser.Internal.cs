using Citron.Collections;
using Citron.LexicalAnalysis;
using Citron.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Citron;

partial struct ExpParser
{
    Lexer lexer;
    ParserContext context;

    public static bool Parse(Lexer lexer, ref ParserContext context, [NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        var parser = new ExpParser { lexer = lexer, context = context };
        if (!parser.ParseExp(out outExp))
            return false;

        context = parser.context;
        return true;
    }

    public static bool ParseCallArgs(Lexer lexer, ref ParserContext context, [NotNullWhen(returnValue: true)] out ImmutableArray<Argument>? outArgs)
    {
        var parser = new ExpParser { lexer = lexer, context = context };
        if (!parser.ParseCallArgs(out outArgs))
            return false;

        context = parser.context;
        return true;
    }

    bool Accept<TToken>([NotNullWhen(true)] out TToken? token) where TToken : Token
    {
        var lexResult = lexer.LexNormalMode(context.LexerContext, true);
        return Accept(lexResult, out token);
    }

    bool Accept<TToken>(LexResult lexResult, [NotNullWhen(true)] out TToken? token) where TToken : Token
    {
        if (lexResult.HasValue && lexResult.Token is TToken resultToken)
        {
            context = context.Update(lexResult.Context);
            token = resultToken;
            return true;
        }

        token = null;
        return false;
    }
    
    bool Peek<TToken>() where TToken : Token
    {
        var lexResult = lexer.LexNormalMode(context.LexerContext, true);
        return lexResult.HasValue && lexResult.Token is TToken;
    }

    delegate bool ParseBaseExpDelegate(ref ExpParser parser, [NotNullWhen(returnValue: true)] out Exp? outExp);

    bool InternalParseLeftAssocBinaryOpExp(
        ParseBaseExpDelegate parseBaseExp, 
        (Token Token, BinaryOpKind OpKind)[] infos,
        [NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        if (!parseBaseExp.Invoke(ref this, out var exp0))
        {
            outExp = null;
            return false;
        }

        while (true)
        {
            BinaryOpKind? opKind = null;

            var lexResult = lexer.LexNormalMode(context.LexerContext, true);
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
            {
                outExp = exp0;
                return true;
            }

            if (!parseBaseExp.Invoke(ref this, out var exp1))
            {
                outExp = null;
                return false;
            }

            // Fold
            exp0 = new BinaryOpExp(opKind.Value, exp0, exp1);
        }
    }

    bool InternalHandleUnaryMinusWithIntLiteral(UnaryOpKind kind, Exp exp, [NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        if (kind == UnaryOpKind.Minus && exp is IntLiteralExp intLiteralExp)
        {
            outExp = new IntLiteralExp(-intLiteralExp.Value);
            return true;
        }

        outExp = null;
        return false;
    }

    #region Single
    bool InternalParseSingleExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        if (ParseNewExp(out outExp))        
            return true;

        if (ParseLambdaExp(out outExp))
            return true;

        if (ParseParenExp(out outExp))
            return true;

        if (ParseNullLiteralExp(out outExp))
            return true;

        if (ParseBoolLiteralExp(out outExp))
            return true;

        if (ParseIntLiteralExp(out outExp))
            return true;

        if (ParseStringExp(out var strExp))
        {
            outExp = strExp;
            return true;
        }

        if (ParseListExp(out outExp))
            return true;

        if (ParseIdentifierExp(out outExp))
            return true;

        outExp = null;
        return false;
    }

    #endregion

    #region Primary, Postfix Inc/Dec       

    bool InternalParseArgument([NotNullWhen(returnValue: true)] out Argument? outArg)
    {
        // params, ref
        if (Accept<ParamsToken>(out _))
        {
            if (!ParseExp(out var exp))
            {
                outArg = null;
                return false;
            }

            outArg = new Argument.Params(exp);
            return true;
        }
        else
        {
            if (!ParseExp(out var exp))
            {
                outArg = null;
                return false;
            }

            outArg = new Argument.Normal(exp);
            return true;
        }
    }

    bool InternalParseCallArgs([NotNullWhen(returnValue: true)] out ImmutableArray<Argument>? outArgs)
    {
        if (!Accept<LParenToken>(out _))
        {
            outArgs = null;
            return false;
        }

        var builder = ImmutableArray.CreateBuilder<Argument>();
        while (!Accept<RParenToken>(out _))
        {
            if (0 < builder.Count)
                if (!Accept<CommaToken>(out _))
                {
                    outArgs = null;
                    return false;
                }

            if (!ParseArgument(out var arg))
            {
                outArgs = null;
                return false;
            }

            builder.Add(arg);
        }

        outArgs = builder.ToImmutable();
        return true;
    }

    static (Token Token, UnaryOpKind OpKind)[] primaryInfos = new (Token Token, UnaryOpKind OpKind)[]
    {
        (PlusPlusToken.Instance, UnaryOpKind.PostfixInc),
        (MinusMinusToken.Instance, UnaryOpKind.PostfixDec),
    };

    // postfix
    bool InternalParsePrimaryExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        bool ParseBaseExp(ref ExpParser parser, [NotNullWhen(returnValue: true)] out Exp? outExp) => parser.ParseSingleExp(out outExp);

        if (!ParseBaseExp(ref this, out var exp))
        {
            outExp = null;
            return false;
        }

        while (true)
        {
            // Unary일수도 있고, ()일수도 있다
            var lexResult = lexer.LexNormalMode(context.LexerContext, true);
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
            if (Accept<LBracketToken>(lexResult, out _))
            {
                if (!ParseExp(out var index))
                {
                    outExp = null;
                    return false;
                }

                if (!Accept<RBracketToken>(out _))
                {
                    outExp = null;
                    return false;
                }

                exp = new IndexerExp(exp, index);
                continue;
            }

            // . id < >
            if (Accept<DotToken>(lexResult, out _))
            {
                if (!Accept<IdentifierToken>(out var idToken))
                {
                    outExp = null;
                    return false;
                }

                // <                    
                if (TypeExpParser.ParseTypeArgs(lexer, ref context, out var typeArgs))
                    exp = new MemberExp(exp, idToken.Value, typeArgs.Value);
                else
                    exp = new MemberExp(exp, idToken.Value, MemberTypeArgs: default);
                continue;
            }

            // (..., ... )                
            if (ParseCallArgs(out var callArgs))
            {
                exp = new CallExp(exp, callArgs.Value);
                continue;
            }

            break;
        }

        outExp = exp;
        return true;
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

    bool InternalParseUnaryExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        bool ParseBaseExp(ref ExpParser parser, [NotNullWhen(returnValue: true)] out Exp? outExp) => parser.ParsePrimaryExp(out outExp);

        UnaryOpKind? opKind = null;

        var lexResult = lexer.LexNormalMode(context.LexerContext, true);
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
            if (!ParseUnaryExp(out var exp))
            {
                outExp = null;
                return false;
            }

            if (HandleUnaryMinusWithIntLiteral(opKind.Value, exp, out var handledExp))
            {
                outExp = handledExp;
                return true;
            }

            outExp = new UnaryOpExp(opKind.Value, exp);
            return true;
        }
        else
        {
            return ParseBaseExp(ref this, out outExp);
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

    bool InternalParseMultiplicativeExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        return ParseLeftAssocBinaryOpExp(
            (ref ExpParser parser, [NotNullWhen(returnValue: true)] out Exp? outExp) => parser.ParseUnaryExp(out outExp), 
            multiplicativeInfos, 
            out outExp);
    }
    #endregion


    #region Additive, LeftAssoc
    static (Token Token, BinaryOpKind OpKind)[] additiveInfos = new (Token Token, BinaryOpKind OpKind)[]
    {
        (PlusToken.Instance, BinaryOpKind.Add),
        (MinusToken.Instance, BinaryOpKind.Subtract),
    };

    bool InternalParseAdditiveExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        return ParseLeftAssocBinaryOpExp(
            (ref ExpParser parser, [NotNullWhen(returnValue: true)] out Exp? outExp) => parser.ParseMultiplicativeExp(out outExp), 
            additiveInfos,
            out outExp) ;
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

    bool InternalParseTestExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        return ParseLeftAssocBinaryOpExp(
            (ref ExpParser parser, [NotNullWhen(returnValue: true)] out Exp? outExp) => parser.ParseAdditiveExp(out outExp), 
            testInfos, 
            out outExp);
    }
    #endregion

    #region Equality, Left Assoc
    static (Token Token, BinaryOpKind OpKind)[] equalityInfos = new (Token Token, BinaryOpKind OpKind)[]
    {
        (EqualEqualToken.Instance, BinaryOpKind.Equal),
        (ExclEqualToken.Instance, BinaryOpKind.NotEqual),
    };

    bool InternalParseEqualityExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        return ParseLeftAssocBinaryOpExp(
            (ref ExpParser parser, [NotNullWhen(returnValue: true)] out Exp? outExp) => parser.ParseTestExp(out outExp), 
            equalityInfos, 
            out outExp);
    }
    #endregion


    #region Assignment, Right Assoc
    bool InternalParseAssignExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        bool ParseBaseExp(ref ExpParser parser, [NotNullWhen(returnValue: true)] out Exp? outExp) => parser.ParseEqualityExp(out outExp);

        if (!ParseBaseExp(ref this, out var exp0))
        {
            outExp = null;
            return false;
        }

        if (!Accept<EqualToken>(out _))
        {
            outExp = exp0;
            return true;
        }

        if (!ParseAssignExp(out var exp1))
        {
            outExp = null;
            return false;
        }

        outExp = new BinaryOpExp(BinaryOpKind.Assign, exp0, exp1);
        return true;
    }

    #endregion

    #region LambdaExpression, Right Assoc
    bool InternalParseLambdaExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        var paramsBuilder = ImmutableArray.CreateBuilder<LambdaExpParam>(); 

        // (), (a, b)            
        // (int a)
        // (ref int a, int b) => ...
        // a
        if (Accept<IdentifierToken>(out var idToken))
        {
            paramsBuilder.Add(new LambdaExpParam(null, idToken.Value));
        }
        else if (Accept<LParenToken>(out _))
        {
            while (!Accept<RParenToken>(out _))
            {
                if (0 < paramsBuilder.Count)
                    if (!Accept<CommaToken>(out _))
                    {
                        outExp = null;
                        return false;
                    }

                // id id or id
                if (!Accept<IdentifierToken>(out var firstIdToken))
                {
                    outExp = null;
                    return false;
                }

                if (!Accept<IdentifierToken>(out var secondIdToken))
                    paramsBuilder.Add(new LambdaExpParam(null, firstIdToken.Value));
                else
                    paramsBuilder.Add(new LambdaExpParam(new IdTypeExp(firstIdToken.Value, default), secondIdToken.Value));
            }
        }

        // =>
        if (!Accept<EqualGreaterThanToken>(out _))
        {
            outExp = null;
            return false;
        }

        // exp => return exp;
        // { ... }
        ImmutableArray<Stmt> body;
        if (Peek<LBraceToken>())
        {
            // Body 파싱을 그대로 쓴다
            if (!StmtParser.ParseBody(lexer, ref context, out var stmtBody))
            {
                outExp = null;
                return false;
            }

            body = stmtBody.Value;
        }
        else
        {
            // exp
            if (!ParseExp(out var expBody))
            {
                outExp = null;
                return false;
            }

            body = ImmutableArray<Stmt>.Empty.Add(new ReturnStmt(new ReturnValueInfo(expBody)));
        }

        outExp = new LambdaExp(paramsBuilder.ToImmutable(), body);
        return true;
    }
    #endregion

    bool InternalParseNewExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        // <NEW> <TYPEEXP> <LPAREN> CallArgs <RPAREN>
        if (!Accept<NewToken>(out _))
        {
            outExp = null;
            return false;
        }

        if (!TypeExpParser.Parse(lexer, ref context, out var type))
        {
            outExp = null;
            return false;
        }

        if (!ParseCallArgs(out var callArgs))
        {
            outExp = null;
            return false;
        }

        outExp = new NewExp(type, callArgs.Value);
        return true;
    }

    bool InternalParseParenExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        if (!Accept<LParenToken>(out _))
        {
            outExp = null;
            return false;
        }

        if (!ParseExp(out var exp))
        {
            outExp = null;
            return false;
        }

        if (!Accept<RParenToken>(out _))
        {
            outExp = null;
            return false;
        }

        outExp = exp;
        return true;
    }

    bool InternalParseNullLiteralExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        if (Accept<NullToken>(out _))
        {
            outExp = NullLiteralExp.Instance;
            return true;
        }

        outExp = null;
        return false;
    }

    bool InternalParseBoolLiteralExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        if (!Accept<BoolToken>(out var boolToken))
        {
            outExp = null;
            return false;
        }

        outExp = new BoolLiteralExp(boolToken.Value);
        return true;
    }

    bool InternalParseIntLiteralExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        if (!Accept<IntToken>(out var intToken))
        {
            outExp = null;
            return false;
        }

        outExp = new IntLiteralExp(intToken.Value);
        return true;
    }

    // 스트링 파싱
    bool InternalParseStringExp([NotNullWhen(returnValue: true)] out StringExp? outStrExp)
    {
        if (!Accept<DoubleQuoteToken>(out _))
        {
            outStrExp = null;
            return false;
        }

        var builder = ImmutableArray.CreateBuilder<StringExpElement>();
        while (!Accept<DoubleQuoteToken>(lexer.LexStringMode(context.LexerContext), out _))
        {
            if (Accept<TextToken>(lexer.LexStringMode(context.LexerContext), out var textToken))
            {
                builder.Add(new TextStringExpElement(textToken.Text));
                continue;
            }

            if (Accept<IdentifierToken>(lexer.LexStringMode(context.LexerContext), out var idToken))
            {
                builder.Add(new ExpStringExpElement(new IdentifierExp(idToken.Value, default)));
                continue;
            }

            // ${
            if (Accept<DollarLBraceToken>(lexer.LexStringMode(context.LexerContext), out _))
            {
                // TODO: EndInnerExpToken 일때 빠져나와야 한다는 표시를 해줘야 한다
                if (!ParseExp(out var exp))
                {
                    outStrExp = null;
                    return false;
                }

                if (!Accept<RBraceToken>(out _))
                {
                    outStrExp = null;
                    return false;
                }

                builder.Add(new ExpStringExpElement(exp));
                continue;
            }

            // 나머지는 에러
            outStrExp = null;
            return false;
        }

        outStrExp = new StringExp(builder.ToImmutable());
        return true;
    }

    bool InternalParseListExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        if (!Accept<LBracketToken>(out _))
        {
            outExp = null;
            return false;
        }

        var builder = ImmutableArray.CreateBuilder<Exp>();
        while (!Accept<RBracketToken>(out _))
        {
            if (0 < builder.Count)
                if (!Accept<CommaToken>(out _))
                {
                    outExp = null;
                    return false;
                }

            if (!ParseExp(out var elem))
            {
                outExp = null;
                return false;
            }

            builder.Add(elem);
        }

        outExp = new ListExp(null, builder.ToImmutable());
        return true;
    }

    bool InternalParseIdentifierExp([NotNullWhen(returnValue: true)] out Exp? outExp)
    {
        if (!Accept<IdentifierToken>(out var idToken))
        {
            outExp = null;
            return false;
        }

        // 실패해도 괜찮다                    
        ImmutableArray<TypeExp> typeArgs = default;
        if (TypeExpParser.ParseTypeArgs(lexer, ref context, out var typeArgsResult))
            typeArgs = typeArgsResult.Value;

        outExp = new IdentifierExp(idToken.Value, typeArgs);
        return true;
    }
}
