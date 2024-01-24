#pragma once
#include "TextAnalysisConfig.h"

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
class Buffer : public std::enable_shared_from_this<Buffer>
{
    std::u32string string;

public:
    TEXT_ANALYSIS_API Buffer(std::u32string string);
    TEXT_ANALYSIS_API BufferPosition MakeStartPosition();

private:
    friend ValidBufferPosition;
};

}