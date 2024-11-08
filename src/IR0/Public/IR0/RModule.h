#pragma once

#include <memory>

#include "RDecl.h"
#include "RFuncDeclOuter.h"
#include "RTypeDeclOuter.h"

namespace Citron {

class MModule;

class RModule
{
public:
};

class RMModule : public RModule
{
    std::shared_ptr<MModule> decl;
    // std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
};


} // namespace Citron