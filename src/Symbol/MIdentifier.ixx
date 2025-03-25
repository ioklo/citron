export module Citron.MSymbol:MIdentifier;

import <vector>;
import <memory>;
import :MNames;

namespace Citron {

using MTypePtr = std::shared_ptr<class MType>;

export struct MIdentifier
{
    MName name;
    int typeParamCount;
    std::vector<MTypePtr> paramIds;
};

}