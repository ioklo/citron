#include "pch.h"
#include "DesignatedErrorLogger.h"
#include <Logging/Logger.h>

namespace Citron::SyntaxIR0Translator {

DesignatedErrorLogger::DesignatedErrorLogger(Logger& logger, void (Logger::*Func)())
    : logger(logger), Func(Func)
{
}

void DesignatedErrorLogger::Log()
{
    (logger.*Func)();
}

} // namespace Citron::SyntaxIR0Translator