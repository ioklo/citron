export module Citron.RDecls:RModule;

import <memory>;

import Citron.MDecls;

import :RDecl;
import :RFuncDeclOuter;
import :RTypeDeclOuter;

namespace Citron {

export class RTypeArguments;
export using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

export class RModule
{
public:
};

export class RMModule : public RModule
{
    std::shared_ptr<MModule> decl;
    // std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron