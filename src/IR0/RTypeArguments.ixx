export module Citron.RDecls:RTypeArguments;

import "IR0Config.h";

import <vector>;
import <memory>;

namespace Citron {

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export class RTypeFactory;

export class RTypeArguments
{
    std::vector<RTypePtr> items;

private:
    friend RTypeFactory;
    RTypeArguments(const std::vector<RTypePtr>& items);

public:
    IR0_API size_t GetCount();
    IR0_API const RTypePtr& Get(int i);
    IR0_API std::shared_ptr<RTypeArguments> Apply(RTypeArguments& typeArgs, RTypeFactory& typeFactory);
};

export using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

} // namespace Citron