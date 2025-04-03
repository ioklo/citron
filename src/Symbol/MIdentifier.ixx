export module Citron.MDecls:MIdentifier;


import <vector>;
import <memory>;

import :MNames;

namespace Citron {

export class MType;
export using MTypePtr = std::shared_ptr<MType>;

export struct MIdentifier
{
    MName name;
    int typeParamCount;
    std::vector<MTypePtr> paramIds;
};

}