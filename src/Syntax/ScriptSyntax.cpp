#include "pch.h"
#include <Syntax/ScriptSyntax.h>

#include <Infra/Json.h>

namespace Citron {

BEGIN_IMPLEMENT_JSON_CLASS(ScriptSyntax)

    IMPLEMENT_JSON_MEMBER(elems)

END_IMPLEMENT_JSON_CLASS()


}