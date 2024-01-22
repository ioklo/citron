#include "pch.h"
#include "Buffer.h"

#include <cassert>
#include "BufferPosition.h"

using namespace std;
using namespace icu;

namespace Citron {

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