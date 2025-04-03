export module Citron.SyntaxIR0Translator:DesignatedErrorLogger;

import <memory>;
import Citron.Logger;

namespace Citron::SyntaxIR0Translator {

export class IDesignatedErrorLogger
{
public:
    virtual void Log() = 0;
};

export struct DesignatedErrorLogger : public IDesignatedErrorLogger
{
    Logger& logger;
    void (Logger::*Func)();

    DesignatedErrorLogger(Logger& logger, void (Logger::*Func)());
    void Log() override;
};

} // namespace Citron::SyntaxIR0Translator