export module Citron.IWriter;

import <string>;

namespace Citron {

export class IWriter
{
public:
    virtual ~IWriter() = default; // default implementation

    virtual void AddIndent() = 0;
    virtual void RemoveIndent() = 0;
    virtual void Write(const std::string& str) = 0;
    virtual void WriteLine() = 0;
};

}
