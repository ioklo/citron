#pragma once
#include "LoggingConfig.h"
#include <memory>

namespace Citron {

using SSyntaxPtr = std::shared_ptr<class SSyntax>;

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

    LOGGING_API void Fatal_TryAccessingPrivateMember(); // A2011_ResolveIdentifier_TryAccessingPrivateMember

    
    LOGGING_API void Fatal_CantInferElementTypeWithEmptyElement(); // A1701_ListExp_CantInferElementTypeWithEmptyElement
    LOGGING_API void Fatal_MismatchBetweenElementTypes();          // A1702_ListExp_MismatchBetweenElementTypes

    LOGGING_API void Fatal_ExpElementShouldBeBoolOrIntOrString(); // A1901_StringExp_ExpElementShouldBeBoolOrIntOrString
    

    LOGGING_API void Fatal_VarWithTypeArg();                     // A2002_ResolveIdentifier_VarWithTypeArg
    LOGGING_API void Fatal_CantGetStaticMemberThroughInstance(); // A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance
    LOGGING_API void Fatal_CantGetTypeMemberThroughInstance();   // A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance
    LOGGING_API void Fatal_CantGetInstanceMemberThroughType();   // A2005_ResolveIdentifier_CantGetInstanceMemberThroughType
    LOGGING_API void Fatal_NotFound();                           // A2007_ResolveIdentifier_NotFound
    LOGGING_API void Fatal_FuncCantHaveMember();                 // A2006_ResolveIdentifier_FuncCantHaveMember
    LOGGING_API void Fatal_CantUseTypeAsExpression();            // A2008_ResolveIdentifier_CantUseTypeAsExpression
    LOGGING_API void Fatal_EnumElemCantHaveMember();             // A2009_ResolveIdentifier_EnumElemCantHaveMember
    LOGGING_API void Fatal_EnumInstanceCantHaveMember();         // A2013_ResolveIdentifier_EnumInstanceCantHaveMember // 2013겹침
    LOGGING_API void Fatal_CantUseNamespaceAsExpression();       // A2013_ResolveIdentifier_CantUseNamespaceAsExpression
    LOGGING_API void Fatal_ExpressionIsNotLocation();            // A2015_ResolveIdentifier_ExpressionIsNotLocation    
    LOGGING_API void Fatal_LambdaInstanceCantHaveMember();       // A2016_ResolveIdentifier_LambdaInstanceCantHaveMember
    LOGGING_API void Fatal_LocalPtrCantHaveMember();             // A2017_ResolveIdentifier_LocalPtrCantHaveMember
    LOGGING_API void Fatal_BoxPtrCantHaveMember();               // A2018_ResolveIdentifier_BoxPtrCantHaveMember
    LOGGING_API void Fatal_FuncInstanceCantHaveMember();         // A2019_ResolveIdentifier_FuncInstanceCantHaveMember
    

    LOGGING_API void Fatal_CastFailed();                   // A2201_Cast_Failed

    LOGGING_API void Fatal_TypeIsNotClass();               // A2601_NewExp_TypeIsNotClass
    LOGGING_API void Fatal_CantInferNullableType();        // A2701_NullLiteralExp_CantInferNullableType

    // IrExp -> RExp Translation
    LOGGING_API void Fatal_CantMakeReference();                 // A3001_Reference_CantMakeReference
    LOGGING_API void Fatal_CantReferenceTempValue();            // A3002_Reference_CantReferenceTempValue
    LOGGING_API void Fatal_UselessDereferenceReferencedValue(); // A3003_Reference_UselessDereferenceReferencedValue
    LOGGING_API void Fatal_CantReferenceThis();                 // A3004_Reference_CantReferenceThis


};

using LoggerPtr = std::shared_ptr<Logger>;

}