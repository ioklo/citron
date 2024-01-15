#include "pch.h"
#include "Syntax/NamespaceDecl.h"
#include "Syntax/NamespaceElements.h"

using namespace std;

namespace Citron::Syntax {

NamespaceDecl::NamespaceDecl(vector<string> names, vector<NamespaceElement> elements)
    : names(std::move(names)), elements(std::move(elements))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(NamespaceDecl)

}