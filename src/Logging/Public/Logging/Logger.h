#pragma once
#include "LoggingConfig.h"
#include <memory>

namespace Citron {

class Logger
{
public:
    
    LOGGING_API void Fatal_CantUseTypeAsExpression();      // A2008_ResolveIdentifier_CantUseTypeAsExpression);
    LOGGING_API void Fatal_CantUseNamespaceAsExpression(); // A2013_ResolveIdentifier_CantUseNamespaceAsExpression

    LOGGING_API void Fatal_ExpressionIsNotLocation();      // A2015_ResolveIdentifier_ExpressionIsNotLocation    

    // IrExp -> RExp Translation
    LOGGING_API void Fatal_CantMakeReference();
    LOGGING_API void Fatal_CantReferenceThis();
    LOGGING_API void Fatal_CantReferenceTempValue(); // A3002_Reference_CantReferenceTempValue
    LOGGING_API void Fatal_UselessDereferenceReferencedValue(); // A3003_Reference_UselessDereferenceReferencedValue
};

using LoggerPtr = std::shared_ptr<Logger>;

}