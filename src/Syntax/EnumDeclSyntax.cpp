#include "pch.h"
#include <Infra/Json.h>
#include <Syntax/EnumDeclSyntax.h>

namespace Citron {

BEGIN_IMPLEMENT_JSON_CLASS(EnumElemMemberVarDeclSyntax)
    IMPLEMENT_JSON_MEMBER(type)
    IMPLEMENT_JSON_MEMBER(name)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(EnumElemDeclSyntax)
    IMPLEMENT_JSON_MEMBER(name)
    IMPLEMENT_JSON_MEMBER(memberVars)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(EnumDeclSyntax)
    IMPLEMENT_JSON_MEMBER(accessModifier)
    IMPLEMENT_JSON_MEMBER(name)
    IMPLEMENT_JSON_MEMBER(typeParams)
    IMPLEMENT_JSON_MEMBER(elems)
END_IMPLEMENT_JSON_CLASS()

}