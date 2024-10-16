#include "pch.h"
#include "Lexer.h"

#include <sstream>
#include <string_view>
#include <vector>
#include <unordered_map>

#include <Syntax/Tokens.h>
#include <cassert>
#include <utf8.h>

using namespace std;
using namespace utf8;

namespace {

using namespace Citron;

struct TokenInfo
{
    const char32_t* sequence;
    Token token;    
};

TokenInfo infos[] = {
    { U"++", PlusPlusToken() },
    { U"--", MinusMinusToken() },
    { U"<=", LessThanEqualToken() },
    { U">=", GreaterThanEqualToken() },
    { U"=>", EqualGreaterThanToken() },
    { U"==", EqualEqualToken() },
    { U"!=", ExclEqualToken() },
    { U"->", MinusGreaterThanToken() },

    { U"@", AtToken() },
    { U"<", LessThanToken() },
    { U">", GreaterThanToken() },
    { U";", SemiColonToken() },
    { U",", CommaToken() },
    { U"=", EqualToken() },
    { U"{", LBraceToken() },
    { U"}", RBraceToken() },
    { U"(", LParenToken() },
    { U")", RParenToken() },
    { U"[", LBracketToken() },
    { U"]", RBracketToken() },


    { U"+", PlusToken() },
    { U"-", MinusToken() },
    { U"*", StarToken() },
    { U"/", SlashToken() },
    { U"%", PercentToken() },
    { U"!", ExclToken() },
    { U".", DotToken() },
    { U"?", QuestionToken() },
    { U"&", AmpersandToken() },

    { U":", ColonToken() },
    { U"`", BacktickToken() },
};

// 키워드 처리
unordered_map<u32string, Token> keywordInfos{
    { U"foreach"s, ForeachToken() },
    { U"if"s, IfToken() },
    { U"else"s, ElseToken() },
    { U"for"s, ForToken() },
    { U"continue"s, ContinueToken() },
    { U"break"s, BreakToken() },
    { U"task"s, TaskToken() },
    { U"params"s, ParamsToken() },
    { U"out"s, OutToken() },
    { U"return"s, ReturnToken() },
    { U"async"s, AsyncToken() },
    { U"await"s, AwaitToken() },
    { U"in"s, InToken() },
    { U"yield"s, YieldToken() },
    { U"seq"s, SeqToken() },
    { U"enum"s, EnumToken() },
    { U"struct"s, StructToken() },
    { U"class"s, ClassToken() },
    { U"is"s, IsToken() },
    { U"as"s, AsToken() },
    { U"ref"s, RefToken() },
    { U"box"s, BoxToken() },
    { U"local"s, LocalToken() },
    { U"null"s, NullToken() },
    { U"public"s, PublicToken() },
    { U"protected"s, ProtectedToken() },
    { U"private"s, PrivateToken() },
    { U"static"s, StaticToken() },
    { U"new"s, NewToken() },
    { U"namespace"s, NamespaceToken() },
};

class BufferIterator
{
    BufferPosition pos;
    ValidBufferPosition* validPos;

public:
    BufferIterator(BufferPosition pos)
        : pos(std::move(pos))
    {
        validPos = get_if<ValidBufferPosition>(&this->pos);
    }

    BufferIterator(const BufferIterator& pos) = default;

    BufferIterator& operator=(BufferIterator&& other)
    {
        pos = std::move(other.pos);
        validPos = get_if<ValidBufferPosition>(&this->pos);
        return *this;
    }

    BufferPosition GetBufferPosition() { return pos; }

    bool IsReachedEnd()
    {
        return holds_alternative<EndBufferPosition>(pos);
    }

    bool Equals(char32_t c)
    {
        return validPos && validPos->Equals(c);
    }

    bool IsWhiteSpaceExceptLineSeparator()
    {
        return validPos && validPos->IsWhiteSpaceExceptLineSeparator();
    }

    bool IsDecimalDigitNumber()
    {
        return validPos && validPos->IsDecimalDigitNumber();
    }

    bool IsIdentifierStartLetter()
    {
        return validPos && validPos->IsIdentifierStartLetter();
    }

    bool IsIdentifierLetter()
    {
        return validPos && validPos->IsIdentifierLetter();
    }

    void AppendTo(u32string& codePoints)
    {
        assert(validPos);
        validPos->AppendTo(codePoints);
    }

    Lexer MakeLexer()
    {
        return Lexer(pos);
    }

    // return false when exception (not reach to end)
    bool Next()
    {
        if (!validPos) return false;

        auto nextPos = validPos->Next();
        if (!nextPos) return false;

        pos = std::move(*nextPos);
        validPos = get_if<ValidBufferPosition>(&pos);
        return true;
    }

    // 성공시 position이 변경
    bool Consume(const char32_t* sequence)
    {
        const char32_t* cp = sequence;
        BufferIterator inner(pos);

        while (*cp)
        {
            if (!inner.Equals(*cp)) return false;
            if (!inner.Next()) return false;
            ++cp;
        }

        pos = std::move(inner.pos);
        return true;
    }
};

optional<LexResult> ResultNextPos(Token token, BufferIterator i)
{
    if (i.IsReachedEnd())
        return LexResult{ std::move(token), i.MakeLexer() };

    if (!i.Next()) return nullopt;

    return LexResult{ std::move(token), i.MakeLexer() };
}

LexResult Result(Token token, Lexer lexer)
{
    return LexResult{ std::move(token), std::move(lexer) };
}

template<typename TInt, typename TString>
TInt ParseInt(TString& str)
{
    TInt res = 0;
    size_t size = str.size();

    for (size_t i = 0; i < size; i++)
        res = res * 10 + (str[i] - '0'); // utf8, utf16, utf32모두 같다

    return res;
}

}

namespace Citron {

Lexer::Lexer(BufferPosition pos)
    : pos(std::move(pos))
{
}

optional<LexResult> Lexer::LexStringMode()
{
    auto oTextResult = LexStringModeText();
    if (oTextResult) return oTextResult;

    BufferIterator i(pos);
    if (i.IsReachedEnd()) return nullopt;

    if (i.Equals(U'"'))
        return ResultNextPos(DoubleQuoteToken(), i);

    if (i.Equals(U'$'))
    {
        if (!i.Next()) return nullopt;

        if (i.Equals(U'{'))
            return ResultNextPos(DollarLBraceToken(), i);

        return i.MakeLexer().LexIdentifier(false);
    }

    return nullopt;
}

optional<LexResult> Lexer::LexStringModeText()
{
    u32string codePoints;

    BufferIterator i(pos);
    while (!i.IsReachedEnd())
    {
        if (i.Equals(U'"')) // "두개 처리
        {
            BufferIterator j = i;

            if (!j.Next()) return nullopt;
            if (!j.Equals(U'"')) break;

            codePoints += U'"';

            i = std::move(j);
            if (!i.Next()) return nullopt;
        }
        else if (i.Equals(U'$')) // $ 처리
        {
            BufferIterator j = i;

            if (!j.Next()) return nullopt;
            if (!j.Equals(U'$')) break;

            codePoints += U'$';

            i = std::move(j);
            if (!i.Next()) return nullopt;
        }
        else
        {
            i.AppendTo(codePoints);
            if (!i.Next()) return nullopt;
        }
    }

    if (codePoints.empty())
        return nullopt; // invalid

    std::string u8token = utf8::utf32to8(codePoints);
    return LexResult { TextToken(std::move(u8token)), i.MakeLexer() };
}

optional<LexResult> Lexer::LexNormalMode(bool bSkipNewLine)
{
    // 스킵처리
    auto oWSResult = LexWhitespace(bSkipNewLine);
    if (oWSResult)
        return oWSResult->lexer.LexNormalModeAfterSkipWhitespace();

    return LexNormalModeAfterSkipWhitespace();
}

optional<LexResult> Lexer::LexNormalModeAfterSkipWhitespace()
{
    // 끝 처리
    BufferIterator i(pos);

    if (i.IsReachedEnd())
        return LexResult{ EndOfFileToken(), i.MakeLexer() };

    // 줄바꿈 문자
    if (auto oNewLineResult = LexNewLine())
        return *oNewLineResult;

    // 여러개 먼저
    if (auto oIntResult = LexInt())
        return *oIntResult;

    if (auto oBoolResult = LexBool())
        return *oBoolResult;

    for(auto& info : infos)
    {
        if (i.Consume(info.sequence))
            return LexResult{ info.token, i.MakeLexer() };
    }

    if (i.Equals(U'"'))
        return ResultNextPos(DoubleQuoteToken(), i);

    if (auto oKeywordResult = LexKeyword())
        return *oKeywordResult;

    // Identifier 시도
    if (auto oIdResult = LexIdentifier(true))
        return *oIdResult;

    return nullopt;
}

optional<LexResult> Lexer::LexCommandMode()
{
    if (auto oNewLineResult = LexNewLine())
        return *oNewLineResult;

    BufferIterator i(pos);

    if (i.IsReachedEnd()) return nullopt;

    // TODO: \} 처리
    if (i.Equals(U'}'))
        return ResultNextPos(RBraceToken(), i);

    if (i.Equals(U'$'))
    {
        BufferIterator j = i;

        if (!j.Next()) return nullopt;
        if (j.IsReachedEnd()) return nullopt; // $로 열어놓고 끝에 도달하면, 에러

        if (j.Equals('{'))
            return ResultNextPos(DollarLBraceToken(), j);

        if (!j.Equals('$'))
        {
            if (auto oIdResult = j.MakeLexer().LexIdentifier(false))
                return *oIdResult;
        }
    }

    u32string codePoints;

    // 나머지는 text모드
    while (!i.IsReachedEnd()) // 끝 도달 전까지
    {
        // NewLine문자
        if (i.Equals(U'\r') || i.Equals(U'\n')) break;

        // TODO: \} 처리
        if (i.Equals(U'}'))
            break;

        if (i.Equals('$'))
        {
            BufferIterator j = i;

            if (!j.Next()) return nullopt;
            if (j.IsReachedEnd()) return nullopt; // $로 열어놓고 끝에 도달하면, 에러

            if (j.Equals(U'$'))
            {
                codePoints += U'$';

                i = std::move(j);
                if (!i.Next()) return nullopt;
                continue;
            }

            break;
        }

        i.AppendTo(codePoints);
        if (!i.Next()) return nullopt;
    }

    if (!codePoints.empty())
        return Result(TextToken(utf32to8(codePoints)), i.MakeLexer());
    else
        return nullopt;
}

bool Lexer::IsReachedEnd()
{
    return holds_alternative<EndBufferPosition>(pos);
}

optional<LexResult> Lexer::LexIdentifier(bool bAllowRawMark)
{
    u32string codePoints;

    BufferIterator i(pos);
    if (i.IsReachedEnd()) return nullopt;

    if (bAllowRawMark && i.Equals(U'@'))
    {
        // eat
    }
    else if (i.IsIdentifierStartLetter())
    {
        i.AppendTo(codePoints);
    }
    else
    {
        return nullopt;
    }    

    while (!i.IsReachedEnd())
    {
        // get next valid position
        if (!i.Next()) return nullopt;

        // append to code point if it is identifier letter
        if (!i.IsIdentifierLetter()) break;
        i.AppendTo(codePoints);
    }

    if (codePoints.empty()) return nullopt;
    return Result(IdentifierToken(utf32to8(codePoints)), i.MakeLexer());
}

optional<LexResult> Lexer::LexKeyword()
{
    u32string codePoints;

    BufferIterator bi(pos);

    while (bi.IsIdentifierLetter())
    {
        bi.AppendTo(codePoints);
        if (!bi.Next()) return nullopt;
    }

    if (codePoints.empty())
        return nullopt;

    auto i = keywordInfos.find(codePoints);
    if (i != keywordInfos.end())
        return Result(i->second, bi.MakeLexer());

    return nullopt;
}

optional<LexResult> Lexer::LexBool()
{
    BufferIterator i(pos);

    if (i.Consume(U"true"))
        return LexResult{ BoolToken(true), i.MakeLexer() };

    if (i.Consume(U"false"))
        return LexResult{ BoolToken(false), i.MakeLexer() };

    return nullopt;
}

optional<LexResult> Lexer::LexInt()
{
    u32string codePoints;

    BufferIterator i(pos);

    while (i.IsDecimalDigitNumber())
    {
        i.AppendTo(codePoints);
        if (!i.Next()) return nullopt;
    }

    if (codePoints.empty())
        return nullopt;

    // TODO: 에러 처리
    int number = ParseInt<int32_t>(codePoints);
    return Result(IntToken(number), i.MakeLexer());
}

optional<LexResult> Lexer::LexWhitespace(bool bIncludeNewLine)
{
    optional<LexResult> nextLineModeFailedResult = nullopt;

    BufferIterator i(pos);

    bool bUpdated = false;
    while (!i.IsReachedEnd())
    {
        if (i.Equals(U'\\'))
        {
            if (bUpdated)
                nextLineModeFailedResult = Result(WhitespaceToken(), i.MakeLexer());
            else
                nextLineModeFailedResult = nullopt;

            if (!i.Next()) return nullopt;
            continue;
        }

        // 코멘트 처리
        bool bComment = i.Consume(U"//");

        if (!bComment)
            bComment = i.Consume(U"#");

        if (bComment)
        {
            while (!i.IsReachedEnd() && !i.Equals(U'\r') && !i.Equals(U'\n'))
            {
                bUpdated = true;
                if (!i.Next()) return nullopt;
            }

            continue;
        }

        if (i.IsWhiteSpaceExceptLineSeparator())
        {
            bUpdated = true;
            if (!i.Next()) return nullopt;
            continue;
        }

        if (bIncludeNewLine && (i.Equals(U'\r') || i.Equals(U'\n')))
        {
            bUpdated = true;

            if (!i.Next()) return nullopt;
            continue;
        }

        if (nextLineModeFailedResult)
        {
            auto oRNPos = i.Consume(U"\r\n");

            if (oRNPos)
            {   
                nextLineModeFailedResult = nullopt;
                bUpdated = true;
                continue;
            }
            else if (i.Equals(U'\r') || i.Equals(U'\n'))
            {
                nextLineModeFailedResult = nullopt;                
                bUpdated = true;

                if (!i.Next()) return nullopt;
                continue;
            }
            else
            {
                // \ 이전을 리턴한다
                return *nextLineModeFailedResult;
            }
        }

        break;
    }

    if (bUpdated)
        return LexResult{ WhitespaceToken(), i.MakeLexer()};
    else
        return nullopt;
}


optional<LexResult> Lexer::LexNewLine()
{
    BufferIterator i(pos);

    bool bUpdated = false;
    while (!i.IsReachedEnd() && (i.Equals(U'\r') || i.Equals(U'\n')))
    {
        bUpdated = true;
        if (!i.Next()) return nullopt;
    }

    if (bUpdated)
        return LexResult{ NewLineToken(), i.MakeLexer() };
    else
        return nullopt;
}



}