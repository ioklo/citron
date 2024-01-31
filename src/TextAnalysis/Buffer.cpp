#include "pch.h"
#include <TextAnalysis/Buffer.h>

#include <cassert>
#include <Unicode/unistr.h>

#include <TextAnalysis/BufferPosition.h>

using namespace std;
using namespace icu;

namespace Citron {

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
    : string(std::move(string))
{
}

BufferPosition Buffer::MakeStartPosition()
{
    if (string.size() == 0) return EndBufferPosition();
    return ValidBufferPosition(weak_from_this(), string[0], 0);
}

}