#pragma once


#include "RDecl.h"
#include "RFuncDeclOuter.h"
#include "RTypeDeclOuter.h"

namespace Citron {

class EModule;

class RTypeArguments;

class RModule
{
public:
};

class REModule : public RModule
{
    EModule* decl;
    // std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron