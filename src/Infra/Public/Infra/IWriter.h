#pragma once
#include <string>

namespace Citron {

class IWriter
{
public:
    virtual ~IWriter() = default; // default implementation

    virtual void AddIndent() = 0;
    virtual void RemoveIndent() = 0;
    virtual void Write(const std::string& str) = 0;
    virtual void WriteLine() = 0;
};

}
