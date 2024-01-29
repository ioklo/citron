#pragma once
#include "SyntaxConfig.h"
#include <Infra/Json.h>
#include <Infra/Unreachable.h>

namespace Citron {

enum class AccessModifierSyntax
{
    Public,
    Protected,
    Private
};

BEGIN_IMPLEMENT_JSON_ENUM_INLINE(AccessModifierSyntax)
    IMPLEMENT_JSON_ENUM(AccessModifierSyntax, Public)
    IMPLEMENT_JSON_ENUM(AccessModifierSyntax, Protected)
    IMPLEMENT_JSON_ENUM(AccessModifierSyntax, Private)
END_IMPLEMENT_JSON_ENUM_INLINE()

}