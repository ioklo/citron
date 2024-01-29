#include "pch.h"
#include <Syntax/NamespaceDeclSyntaxElements.h>

namespace Citron {

BEGIN_IMPLEMENT_JSON_CLASS(GlobalFuncDeclNamespaceDeclSyntaxElement)
    IMPLEMENT_JSON_MEMBER(funcDecl)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(NamespaceDeclNamespaceDeclSyntaxElement)
    IMPLEMENT_JSON_MEMBER(namespaceDecl)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(TypeDeclNamespaceDeclSyntaxElement)
    IMPLEMENT_JSON_MEMBER(typeDecl)
END_IMPLEMENT_JSON_CLASS()


JsonItem ToJson(NamespaceDeclSyntaxElement& syntax)
{
    return std::visit([](auto&& elem) { return elem.ToJson(); }, syntax);
}

}