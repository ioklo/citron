#pragma once

#include <memory>
#include <expected>

#include "RSymbol/RNames.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class RType_Class;
class RType_Struct;
class RClassVarDecl;
class RStructVarDecl;
class RTypeArguments;

struct Result_GetClassVar
{   
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;
};

struct Result_GetStructVar
{
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;
};

std::expected<Result_GetClassVar, DiagPtr> GetClassVar(RType_Class* classType, const RName& name, RTypeArguments* typeArgsExceptOuter, bool bExpectedStatic);
std::expected<Result_GetStructVar, DiagPtr> GetStructVar(RType_Struct* structType, const RName& name, RTypeArguments* typeArgsExceptOuter, bool bExpectedStatic);


} // namespace Citron
