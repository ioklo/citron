#pragma once
#include "RSymbolConfig.h"

#include <span>

#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "RThisKind.h"

namespace Citron {

class RDecl;
class RType;
class RTypeArguments;
class RTypeParamDecl;

class RGlobalFuncDecl;
class RClassCtorDecl;
class RClassFuncDecl;
class RStructCtorDecl;
class RStructDtorDecl;
class RStructFuncDecl;
class RLambdaDecl;

using RFuncDecl = std::variant<
    RGlobalFuncDecl*,
    RClassCtorDecl*,
    RClassFuncDecl*,
    RStructCtorDecl*,
    RStructDtorDecl*,
    RStructFuncDecl*,
    RLambdaDecl*
>;

RSYMBOL_API RDecl* GetRDecl(RFuncDecl& funcDecl);
RSYMBOL_API RThisKind GetThisKind(RFuncDecl& funcDecl);
RSYMBOL_API size_t GetTypeParamCount(RFuncDecl& funcDecl);
RSYMBOL_API RTypeParamDecl* GetTypeParam(RFuncDecl& funcDecl, size_t index);
RSYMBOL_API size_t GetParamCount(RFuncDecl& funcDecl);
RSYMBOL_API RType* GetReturnType(RFuncDecl& funcDecl, RTypeArguments* typeArgs);
RSYMBOL_API RFuncReturn GetFuncReturn(RFuncDecl& funcDecl, RTypeArguments* typeArgs);
RSYMBOL_API RFuncParameter GetFuncParam(RFuncDecl& funcDecl, RTypeArguments* typeArgs, size_t index);
RSYMBOL_API RFuncReturn GetUnboundFuncReturn(RFuncDecl& funcDecl);
RSYMBOL_API std::span<RFuncParameter> GetUnboundFuncParams(RFuncDecl& funcDecl);

} // namespace Citron

