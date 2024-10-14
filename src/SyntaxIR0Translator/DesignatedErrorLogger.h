#pragma once

#include <memory>

namespace Citron {

using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

class IDesignatedErrorLogger
{
public:
    virtual void Log() = 0;
};

struct DesignatedErrorLogger : public IDesignatedErrorLogger
{
    Logger& logger;
    void (Logger::*Func)();

    DesignatedErrorLogger(Logger& logger, void (Logger::*Func)());
    void Log() override;
};

} // namespace SyntaxIR0Translator

} // namespace Citron