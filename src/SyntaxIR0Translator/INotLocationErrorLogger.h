#pragma once

namespace Citron::SyntaxIR0Translator {

class INotLocationErrorLogger
{
public:
    virtual void Log() = 0;
};

}
