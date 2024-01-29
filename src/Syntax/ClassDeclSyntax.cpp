#include "pch.h"
#include <Syntax/ClassDeclSyntax.h>
#include <Infra/Json.h>

namespace Citron {

BEGIN_IMPLEMENT_JSON_CLASS(ClassDeclSyntax)
    IMPLEMENT_JSON_MEMBER(accessModifier)
    IMPLEMENT_JSON_MEMBER(name)
    IMPLEMENT_JSON_MEMBER(typeParams)
    IMPLEMENT_JSON_MEMBER(baseTypes)
    IMPLEMENT_JSON_MEMBER(memberDecls)
END_IMPLEMENT_JSON_CLASS()

}