#include "pch.h"
#include <Syntax/ScriptSyntaxElements.h>

namespace Citron {

BEGIN_IMPLEMENT_JSON_CLASS(NamespaceDeclScriptSyntaxElement)
    IMPLEMENT_JSON_MEMBER(namespaceDecl)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(GlobalFuncDeclScriptSyntaxElement)
    IMPLEMENT_JSON_MEMBER(funcDecl)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(TypeDeclScriptSyntaxElement)
    IMPLEMENT_JSON_MEMBER(typeDecl)
END_IMPLEMENT_JSON_CLASS()

SYNTAX_API JsonItem ToJson(ScriptSyntaxElement& syntax)
{
    return std::visit([](auto&& elem) { return elem.ToJson(); }, syntax);
}

}