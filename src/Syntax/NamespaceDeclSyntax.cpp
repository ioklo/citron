#include "pch.h"
#include <Syntax/NamespaceDeclSyntax.h>
#include <Syntax/NamespaceElementSyntaxes.h>

using namespace std;

namespace Citron {

NamespaceDeclSyntax::NamespaceDeclSyntax(vector<u32string> names, vector<NamespaceElementSyntax> elements)
    : names(std::move(names)), elements(std::move(elements))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(NamespaceDeclSyntax)

}