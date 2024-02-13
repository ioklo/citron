#include "pch.h"
#include <Syntax/Syntaxes.g.h>

namespace Citron {

ArgumentSyntax::ArgumentSyntax(ExpSyntax exp)
    : ArgumentSyntax(false, false, std::move(exp))
{
}

}