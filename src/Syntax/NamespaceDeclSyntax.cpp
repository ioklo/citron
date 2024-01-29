#include "pch.h"
#include <Syntax/NamespaceDeclSyntax.h>
#include <Syntax/NamespaceDeclSyntaxElements.h>

using namespace std;

namespace Citron {

NamespaceDeclSyntax::NamespaceDeclSyntax(vector<u32string> names, vector<NamespaceDeclSyntaxElement> elements)
    : names(std::move(names)), elements(std::move(elements))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(NamespaceDeclSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(NamespaceDeclSyntax)
    IMPLEMENT_JSON_MEMBER(names)
    IMPLEMENT_JSON_MEMBER(elements)
END_IMPLEMENT_JSON_CLASS()

}