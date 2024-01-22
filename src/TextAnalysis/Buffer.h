#pragma once
#include <vector>
#include <optional>
#include <unordered_map>
#include <variant>
#include <unicode/brkiter.h>

namespace Citron {

class ValidBufferPosition;
class EndBufferPosition;

using BufferPosition = std::variant<ValidBufferPosition, EndBufferPosition>;

// 필요없지 않은가, 일단 래핑
class Buffer : std::enable_shared_from_this<Buffer>
{
    std::u32string string;

public:
    Buffer(std::u32string string);
    BufferPosition MakeStartPosition();

private:
    friend ValidBufferPosition;
};

}