export module Citron.Buffer;

import "TextAnalysisConfig.h";

import <vector>;
import <optional>;
import <unordered_map>;
import <variant>;
import <memory>;
import <string>;

namespace Citron {

class Buffer;
class ValidBufferPosition;
class EndBufferPosition;
using BufferPosition = std::variant<ValidBufferPosition, EndBufferPosition>;

// 상태: 
//   - 가리키고 있는 상태 (코드포인트와 다음 인덱스가 있다) => next(가리키는 상태) => 가리키는 상태 or 끝
//   - 끝(끝을 가리키고 있다) => next(끝) => 에러

export class ValidBufferPosition
{
    std::weak_ptr<Buffer> weakBuffer;
    char32_t codePoint;
    int curIndex;

public:
    TEXTANALYSIS_API ValidBufferPosition(std::weak_ptr<Buffer> weakBuffer, char32_t codePoint, int curIndex);

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

export class EndBufferPosition
{
    // no next
};

export using BufferPosition = std::variant<ValidBufferPosition, EndBufferPosition>;

// 필요없지 않은가, 일단 래핑
export class Buffer : public std::enable_shared_from_this<Buffer>
{
    std::u32string string;

public:
    TEXTANALYSIS_API Buffer(std::string string);
    TEXTANALYSIS_API Buffer(std::u32string string);
    TEXTANALYSIS_API BufferPosition MakeStartPosition();

private:
    friend ValidBufferPosition;
};

}