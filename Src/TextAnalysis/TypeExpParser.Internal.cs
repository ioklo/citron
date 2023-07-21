using Citron.LexicalAnalysis;
using Citron.Syntax;
using Citron.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Citron;

partial struct TypeExpParser
{
    Lexer lexer;
    ParserContext context;

    public static bool Parse(Lexer lexer, ParserContext context, [NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var parser = new TypeExpParser { lexer = lexer, context = context };
        return parser.ParseTypeExp0(out outTypeExp);
    }

    public static bool ParseTypeArgs(Lexer lexer, ParserContext context, [NotNullWhen(returnValue: true)] out ImmutableArray<TypeExp>? typeArgs)
    {
        var parser = new TypeExpParser { lexer = lexer, context = context };
        return parser.ParseTypeArgs(out typeArgs);
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

        if (!Accept<LessThanToken>(out _))
        {
            outTypeArgs = null;
            return false;
        }

        while (!Accept<GreaterThanToken>(out _))
        {
            if (0 < typeArgsBuilder.Count)
                if (!Accept<CommaToken>(out _))
                    throw new ParseFatalException();

            if (!ParseTypeExp0(out var typeArg))
                throw new ParseFatalException();

            typeArgsBuilder.Add(typeArg);
        }

        outTypeArgs = typeArgsBuilder.ToImmutable();
        return true;
    }

    bool InternalParseTypeIdExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
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
    
    // 최상위
    bool InternalParseTypeExp0([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        if (!ParseTypeExp1(out var typeExp))
        {
            outTypeExp = null;
            return false;
        }

        // ?를 만났으면
        if (Accept<QuestionToken>(out _))
        {
            outTypeExp = new NullableTypeExp(typeExp);
            return true;
        }
        else
        {
            outTypeExp = typeExp;
            return true;
        }
    }
    
    // 첫번째, box와 *****
    bool InternalParseTypeExp1([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        TypeExp curTypeExp;
        
        // 
        if (Accept<BoxToken>(out _))
        {
            if (!ParseTypeExp2(out var typeExp2))
            {
                outTypeExp = null;
                return false;
            }

            if (!Accept<StarToken>(out _))
            {
                outTypeExp = null;
                return false;
            }

            curTypeExp = new BoxPtrTypeExp(typeExp2);            
        }
        else if (ParseTypeExp2(out var typeExp2))
        {
            curTypeExp = typeExp2;
        }
        else
        {
            outTypeExp = null;
            return false;
        }

        while (Accept<StarToken>(out _))
        {
            curTypeExp = new LocalPtrTypeExp(curTypeExp);
        }

        outTypeExp = curTypeExp;
        return true;
    }

    bool InternalParseTypeExp2([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        TypeExp curTypeExp;
        if (ParseTypeIdExp( out var typeIdExp))
        {
            curTypeExp = typeIdExp;
        }
        else if (Accept<LParenToken>(out _))
        {
            if (!ParseTypeExp0(out var typeExp))
            {
                outTypeExp = null;
                return false;
            }

            if (!Accept<RParenToken>(out _))
            {
                outTypeExp = null;
                return false;
            }

            curTypeExp = typeExp;
        }
        else
        {
            outTypeExp = null;
            return false;
        }

        // .
        while (Accept<DotToken>(out _))
        {
            // ID
            if (!Accept<IdentifierToken>(out var memberName))
            {
                outTypeExp = null;
                return false;
            }

            // TODO: typeApp(T.S<>) 처리도 추가
            curTypeExp = new MemberTypeExp(curTypeExp, memberName.Value, default);
        }

        outTypeExp = curTypeExp;
        return true;
    }
}