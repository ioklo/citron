#pragma once
#include "SyntaxConfig.h"
#include "ExpSyntaxes.h"
#include <Infra/Json.h>

namespace Citron {

struct ArgumentSyntax
{
    bool bOut;
    bool bParams;
    ExpSyntax exp;

    ArgumentSyntax(bool bOut, bool bParams, ExpSyntax exp)
        : bOut(bOut), bParams(bParams), exp(std::move(exp))
    {
    }

    ArgumentSyntax(ExpSyntax exp)
        : bOut(false), bParams(false), exp(std::move(exp)) { }
};

BEGIN_IMPLEMENT_JSON_STRUCT_INLINE(ArgumentSyntax, syntax)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, bOut)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, bParams)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, exp)
END_IMPLEMENT_JSON_STRUCT_INLINE()


} // namespace Citron
 