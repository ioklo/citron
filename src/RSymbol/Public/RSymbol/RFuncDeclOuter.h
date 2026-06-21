#pragma once
#include "RSymbolConfig.h"

#include <variant>

namespace Citron {

class EFuncDeclOuter;

class RDecl;
class RNamespaceDecl;
class RGlobalFuncDecl;
class RClassDecl;
class RClassCtorDecl;
class RClassFuncDecl;
class RStructDecl;
class RStructCtorDecl;
class RStructDtorDecl;
class RStructFuncDecl;
class RLambdaDecl;

using RFuncDeclOuter = std::variant<
    RNamespaceDecl*,
    RGlobalFuncDecl*,
    RClassDecl*,
    RClassCtorDecl*,
    RClassFuncDecl*,
    RStructDecl*,
    RStructCtorDecl*,
    RStructDtorDecl*,
    RStructFuncDecl*,
    RLambdaDecl*>;

RSYMBOL_API RDecl* GetRDecl(RFuncDeclOuter& outer);

} // namespace Citron
