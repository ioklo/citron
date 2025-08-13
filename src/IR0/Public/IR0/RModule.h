#pragma once


#include "RDecl.h"
#include "RFuncDeclOuter.h"
#include "RTypeDeclOuter.h"

namespace Citron {

class MModule;

class RTypeArguments;

class RModule
{
public:
};

class RMModule : public RModule
{
    MModule* decl;
    // std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron