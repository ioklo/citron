#include "pch.h"
#include "NotLocationErrorLogger.h"
#include <Logging/Logger.h>

namespace Citron::SyntaxIR0Translator {

ExpressionIsNotLocationErrorLogger::ExpressionIsNotLocationErrorLogger(const LoggerPtr& logger)
    : logger(logger)
{
}

void ExpressionIsNotLocationErrorLogger::Log()
{ 
    logger->Fatal_ExpressionIsNotLocation(); 
}


NotLocationErrorLogger::NotLocationErrorLogger(const LoggerPtr& logger, void (Logger::*Func)())
    : logger(logger), Func(Func)
{
}

void NotLocationErrorLogger::Log()
{
    (logger.get()->*Func)();
}

} // namespace Citron::SyntaxIR0Translator