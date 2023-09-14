using Citron.LexicalAnalysis;
using Citron.Syntax;
using Citron.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Citron;

partial struct TypeExpParser
{
    Lexer lexer;
    ParserContext context;

    public static bool Parse(Lexer lexer, ref ParserContext context, [NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var parser = new TypeExpParser { lexer = lexer, context = context };
        if (!parser.ParseTypeExp(out outTypeExp))        
            return false;

        context = parser.context;
        return true;
    }

    public static bool ParseTypeArgs(Lexer lexer, ref ParserContext context, [NotNullWhen(returnValue: true)] out ImmutableArray<TypeExp>? typeArgs)
    {
        var parser = new TypeExpParser { lexer = lexer, context = context };
        if (!parser.ParseTypeArgs(out typeArgs))
            return false;

        context = parser.context;
        return true;
    }

    bool Accept(SingleToken token)
    {
        var lexResult = lexer.LexNormalMode(context.LexerContext, true);
        return Accept(token, lexResult);
    }

    bool Accept(SingleToken token, LexResult lexResult)
    {
        if (lexResult.HasValue && lexResult.Token == token)
        {
            context = context.Update(lexResult.Context);
            return true;
        }

        return false;
    }

    bool Accept<TToken>([NotNullWhen(true)] out TToken? token) where TToken : Token
    {
        var lexResult = lexer.LexNormalMode(context.LexerContext, true);

        if (lexResult.HasValue && lexResult.Token is TToken resultToken)
        {
            context = context.Update(lexResult.Context);
            token = resultToken;
            return true;
        }

        token = null;
        return false;
    }

    bool InternalParseTypeArgs([NotNullWhen(returnValue: true)] out ImmutableArray<TypeExp>? outTypeArgs)
    {
        var typeArgsBuilder = ImmutableArray.CreateBuilder<TypeExp>();

        if (!Accept(Tokens.LessThan))
        {
            outTypeArgs = null;
            return false;
        }

        while (!Accept(Tokens.GreaterThan))
        {
            if (0 < typeArgsBuilder.Count)
                if (!Accept(Tokens.Comma))
                {
                    outTypeArgs = null;
                    return false;
                }

            if (!ParseTypeExp(out var typeArg))
            {
                outTypeArgs = null;
                return false;
            }

            typeArgsBuilder.Add(typeArg);
        }

        outTypeArgs = typeArgsBuilder.ToImmutable();
        return true;
    }

    bool InternalParseIdTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        if (!Accept<IdentifierToken>(out var idToken))
        {
            outTypeExp = null;
            return false;
        }

        if (ParseTypeArgs(out var typeArgs))
            outTypeExp = new IdTypeExp(idToken.Value, typeArgs.Value);
        else 
            outTypeExp = new IdTypeExp(idToken.Value, TypeArgs: default);

        return true;
    }

    // T?
    bool InternalParseNullableTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        TypeExp? typeExp;
        if (!ParseBoxPtrTypeExp(out typeExp) &&
            !ParseLocalPtrTypeExp(out typeExp) &&
            !ParseParenTypeExp(out typeExp) &&
            !ParseIdChainTypeExp(out typeExp))
        {
            outTypeExp = null;
            return false;
        }

        if (!Accept(Tokens.Question))
        {
            outTypeExp = null;
            return false;
        }

        outTypeExp = new NullableTypeExp(typeExp);
        return true;
    }

    // box T*
    bool InternalParseBoxPtrTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        if (!Accept(Tokens.Box))
        {
            outTypeExp = null;
            return false;
        }

        TypeExp? typeExp;
        if (!ParseParenTypeExp(out typeExp) &&
            !ParseIdChainTypeExp(out typeExp))
        {
            outTypeExp = null;
            return false;
        }

        if (!Accept(Tokens.Star))
        {
            outTypeExp = null;
            return false;
        }
        
        outTypeExp = new BoxPtrTypeExp(typeExp);
        return true;
    }

    // T*
    bool InternalParseLocalPtrTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        // avoid left recursion

        TypeExp? typeExp;
        if (!ParseParenTypeExp(out typeExp) &&
            !ParseIdChainTypeExp(out typeExp))
        {
            outTypeExp = null;
            return false;
        }

        // 적어도 한개는 있어야 한다
        if (!Accept(Tokens.Star))
        {
            outTypeExp = null;
            return false;
        }

        typeExp = new LocalPtrTypeExp(typeExp);

        while(Accept(Tokens.Star))        
            typeExp = new LocalPtrTypeExp(typeExp);

        outTypeExp = typeExp;
        return true;
    }

    // (T)
    bool InternalParseParenTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        if (!Accept(Tokens.LParen))
        {
            outTypeExp = null;
            return false;
        }

        if (!ParseNullableTypeExp(out outTypeExp) &&
            !ParseBoxPtrTypeExp(out outTypeExp) &&
            !ParseLocalPtrTypeExp(out outTypeExp))
        {
            outTypeExp = null;
            return false;
        }

        if (!Accept(Tokens.RParen))
        {
            outTypeExp = null;
            return false;
        }

        return true;
    }

    // ID...
    bool InternalParseIdChainTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        if (ParseIdTypeExp(out var typeIdExp))
        {
            var curTypeExp = typeIdExp;

            // .
            while (Accept(Tokens.Dot))
            {
                // ID
                if (!Accept<IdentifierToken>(out var memberName))
                {
                    outTypeExp = null;
                    return false;
                }

                ParseTypeArgs(out var typeArgs);
                curTypeExp = new MemberTypeExp(curTypeExp, memberName.Value, typeArgs.GetValueOrDefault());
            }

            outTypeExp = curTypeExp;
            return true;
        }

        outTypeExp = null;
        return false;
    }

    // func<>
    // bool InternalParseFuncTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp);

    // tuple
    // bool InternalParseTupleTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp);

    // 
    bool InternalParseTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        return ParseNullableTypeExp(out outTypeExp) ||
            ParseBoxPtrTypeExp(out outTypeExp) ||
            ParseLocalPtrTypeExp(out outTypeExp) ||
            ParseIdChainTypeExp(out outTypeExp);
    }
}