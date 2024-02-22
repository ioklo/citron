#pragma once
#include "TextAnalysisConfig.h"

#include <memory>
#include <variant>
#include <optional>
#include <vector>
#include <string>

namespace Citron {

class Buffer;

// 상태: 
//   - 가리키고 있는 상태 (코드포인트와 다음 인덱스가 있다) => next(가리키는 상태) => 가리키는 상태 or 끝
//   - 끝(끝을 가리키고 있다) => next(끝) => 에러

class ValidBufferPosition;
class EndBufferPosition;

// forward declaration
using BufferPosition = std::variant<ValidBufferPosition, EndBufferPosition>;

class ValidBufferPosition
{ 
    std::weak_ptr<Buffer> weakBuffer;
    char32_t codePoint; 
    int curIndex;

public:    
    TEXT_ANALYSIS_API ValidBufferPosition(std::weak_ptr<Buffer> weakBuffer, char32_t codePoint, int curIndex);
    
    bool Equals(char32_t codePoint)
    {
        return this->codePoint == codePoint;
    }

    bool IsWhiteSpaceExceptLineSeparator();
    bool IsIdentifierStartLetter();
    bool IsIdentifierLetter();

    bool IsDecimalDigitNumber();

    void AppendTo(std::u32string& codePoints);

    std::optional<BufferPosition> Next();
};

class EndBufferPosition 
{
    // no next
};

}