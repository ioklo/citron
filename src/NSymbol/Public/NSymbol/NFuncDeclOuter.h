#pragma once
#include "NSymbolConfig.h"
#include <variant>

namespace Citron {

class NDecl;
class NNamespaceDecl;
class NGlobalFuncDecl;
class NClassDecl;
class NClassCtorDecl;
class NClassFuncDecl;
class NStructDecl;
class NStructCtorDecl;
class NStructDtorDecl;
class NStructFuncDecl;
class NLambdaDecl;

using NFuncDeclOuter = std::variant<
    NNamespaceDecl*,
    NGlobalFuncDecl*,
    NClassDecl*,
    NClassCtorDecl*,
    NClassFuncDecl*,
    NStructDecl*,
    NStructCtorDecl*,
    NStructDtorDecl*,
    NStructFuncDecl*,
    NLambdaDecl*>;

NSYMBOL_API NDecl* GetNDecl(NFuncDeclOuter& outer);

}