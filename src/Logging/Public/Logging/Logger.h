#pragma once
#include "LoggingConfig.h"
#include <memory>

namespace Citron {

class SSyntax;
using SSyntaxPtr = std::shared_ptr<SSyntax>;

class Logger
{
public:
    // 호출자에서 Set하는 것으로 한다. 함수가 끝나도 Syntax가 지속될 수 있기 때문이다
    LOGGING_API void SetSyntax(SSyntaxPtr syntax);

    LOGGING_API void Fatal_IntTypeIsAllowedOnly(); // A0601_UnaryAssignOp_IntTypeIsAllowedOnly
    LOGGING_API void Fatal_AssignableExpressionIsAllowedOnly(); // A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly

    LOGGING_API void Fatal_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly(); // A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly
    LOGGING_API void Fatal_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly(); // A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly

    LOGGING_API void Fatal_OperatorNotFound(); // A0802_BinaryOp_OperatorNotFound
    LOGGING_API void Fatal_LeftOperandIsNotAssignable(); // A0803_BinaryOp_LeftOperandIsNotAssignable

    LOGGING_API void Fatal_CantInferElementTypeWithEmptyElement(); // A1701_ListExp_CantInferElementTypeWithEmptyElement
    LOGGING_API void Fatal_MismatchBetweenElementTypes();          // A1702_ListExp_MismatchBetweenElementTypes

    LOGGING_API void Fatal_ExpElementShouldBeBoolOrIntOrString(); // A1901_StringExp_ExpElementShouldBeBoolOrIntOrString
    
    LOGGING_API void Fatal_CantUseTypeAsExpression();      // A2008_ResolveIdentifier_CantUseTypeAsExpression);
    LOGGING_API void Fatal_CantUseNamespaceAsExpression(); // A2013_ResolveIdentifier_CantUseNamespaceAsExpression

    LOGGING_API void Fatal_ExpressionIsNotLocation();      // A2015_ResolveIdentifier_ExpressionIsNotLocation    

    LOGGING_API void Fatal_CastFailed();                   // A2201_Cast_Failed

    LOGGING_API void Fatal_TypeIsNotClass();               // A2601_NewExp_TypeIsNotClass
    LOGGING_API void Fatal_CantInferNullableType();        // A2701_NullLiteralExp_CantInferNullableType

    // IrExp -> RExp Translation
    LOGGING_API void Fatal_CantMakeReference();
    LOGGING_API void Fatal_CantReferenceThis();
    LOGGING_API void Fatal_CantReferenceTempValue(); // A3002_Reference_CantReferenceTempValue
    LOGGING_API void Fatal_UselessDereferenceReferencedValue(); // A3003_Reference_UselessDereferenceReferencedValue


};

using LoggerPtr = std::shared_ptr<Logger>;

}