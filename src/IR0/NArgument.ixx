export module Citron.NDecls:NArgument;

import <variant>;
import <memory>;

namespace Citron {

export class NExp;
export using NExpPtr = std::shared_ptr<NExp>;

export struct NArgument_Normal
{
    NExpPtr exp;
};

export struct NArgument_Params
{
    NExpPtr exp;
    int elemCount;
};

export using NArgument = std::variant<NArgument_Normal, NArgument_Params>;

}