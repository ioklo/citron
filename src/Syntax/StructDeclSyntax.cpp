#include "pch.h"

#include <Infra/Json.h>
#include <Syntax/StructDeclSyntax.h>


namespace Citron {

BEGIN_IMPLEMENT_JSON_CLASS(StructDeclSyntax)
    IMPLEMENT_JSON_MEMBER(accessModifier)
    IMPLEMENT_JSON_MEMBER(name)
    IMPLEMENT_JSON_MEMBER(typeParams)
    IMPLEMENT_JSON_MEMBER(baseTypes)
    IMPLEMENT_JSON_MEMBER(memberDecls)
END_IMPLEMENT_JSON_CLASS()

}