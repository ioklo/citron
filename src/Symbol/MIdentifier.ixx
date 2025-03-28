export module Citron.MSymbol:MIdentifier;


import <vector>;
import <memory>;

import :ForwardDecls;
import Citron.MNames;

namespace Citron {

export struct MIdentifier
{
    MName name;
    int typeParamCount;
    std::vector<MTypePtr> paramIds;
};

}