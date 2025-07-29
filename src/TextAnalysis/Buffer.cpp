#include "Buffer.h"

#include <cassert>
#include <Unicode/unistr.h>
#include <Unicode/uchar.h>

using namespace std;
using namespace icu;

namespace Citron {

ValidBufferPosition::ValidBufferPosition(weak_ptr<Buffer> weakBuffer, char32_t codePoint, int curIndex)
    : weakBuffer(move(weakBuffer)), codePoint(codePoint), curIndex(curIndex)
{
}

bool ValidBufferPosition::IsWhiteSpaceExceptLineSeparator()
{
    return codePoint != U'\r' && codePoint != U'\n' && u_isUWhiteSpace(codePoint);
}

bool ValidBufferPosition::IsIdentifierStartLetter()
{
    if (codePoint == U'_') return true; // only allowed among ConnectorPunctuation category

    int8_t category = u_charType(codePoint);

    return category == U_UPPERCASE_LETTER ||
        category == U_LOWERCASE_LETTER ||
        category == U_TITLECASE_LETTER ||
        category == U_MODIFIER_LETTER ||
        category == U_OTHER_LETTER ||
        category == U_NON_SPACING_MARK ||
        category == U_LETTER_NUMBER;
}

bool ValidBufferPosition::IsIdentifierLetter()
{
    if (codePoint == U'_') return true; // only allowed among ConnectorPunctuation category

    int8_t category = u_charType(codePoint);

    return category == U_UPPERCASE_LETTER ||
        category == U_LOWERCASE_LETTER ||
        category == U_TITLECASE_LETTER ||
        category == U_MODIFIER_LETTER ||
        category == U_OTHER_LETTER ||
        category == U_NON_SPACING_MARK ||
        category == U_LETTER_NUMBER ||
        category == U_DECIMAL_DIGIT_NUMBER; // allow digit number
}

bool ValidBufferPosition::IsDecimalDigitNumber()
{
    return u_charType(codePoint) == U_DECIMAL_DIGIT_NUMBER;
}

void ValidBufferPosition::AppendTo(u32string& codePoints)
{
    codePoints += codePoint;
}

optional<BufferPosition> ValidBufferPosition::Next()
{
    auto buffer = weakBuffer.lock();
    if (!buffer) return nullopt;

    int nextIndex = curIndex + 1;
    if (buffer->string.size() <= nextIndex)
        return EndBufferPosition();

    return ValidBufferPosition(weakBuffer, buffer->string[nextIndex], nextIndex);
}

Buffer::Buffer(std::string str8) // utf-8
{
    auto ustr = UnicodeString::fromUTF8(str8);
    auto size = ustr.countChar32();

    string.resize(size);
    
    UErrorCode errorCode;
    auto resultSize = ustr.toUTF32((UChar32*)string.data(), size, errorCode);
    assert(resultSize == size);
}

Buffer::Buffer(std::u32string string)
    : string(move(string))
{
}

BufferPosition Buffer::MakeStartPosition()
{
    if (string.size() == 0) return EndBufferPosition();
    return ValidBufferPosition(weak_from_this(), string[0], 0);
}

}