#pragma once

#include <memory>

namespace Citron {

using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

class INotLocationErrorLogger
{
public:
    virtual void Log() = 0;
};

struct ExpressionIsNotLocationErrorLogger : public INotLocationErrorLogger
{
    LoggerPtr logger;
    ExpressionIsNotLocationErrorLogger(const LoggerPtr& logger);
    void Log() override;
};

struct NotLocationErrorLogger : public INotLocationErrorLogger
{
    LoggerPtr logger;
    void (Logger::*Func)();

    NotLocationErrorLogger(const LoggerPtr& logger, void (Logger::*Func)());
    void Log() override;
};

} // namespace SyntaxIR0Translator

} // namespace Citron