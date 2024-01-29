#include "pch.h"
#include <Syntax/GlobalFuncDeclSyntax.h>

namespace Citron {

BEGIN_IMPLEMENT_JSON_CLASS(GlobalFuncDeclSyntax)
    IMPLEMENT_JSON_MEMBER(accessModifier)
    IMPLEMENT_JSON_MEMBER(bSequence) // seq 함수인가        
    IMPLEMENT_JSON_MEMBER(retType)
    IMPLEMENT_JSON_MEMBER(name)
    IMPLEMENT_JSON_MEMBER(typeParams)
    IMPLEMENT_JSON_MEMBER(parameters)
    IMPLEMENT_JSON_MEMBER(body)
END_IMPLEMENT_JSON_CLASS()

}