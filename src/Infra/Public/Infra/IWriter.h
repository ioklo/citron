#pragma once

#include <string>
#include <string_view>

namespace Citron {

class IWriter
{
public:
    virtual ~IWriter() = default; // default implementation

    virtual void AddIndent() = 0;
    virtual void RemoveIndent() = 0;
    virtual void Write(std::string_view str) = 0;
    virtual void WriteLine() = 0;
};

}
