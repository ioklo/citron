module;
#include "Macros.h"

export module Citron.Identifiers:Identifier;

import <vector>;

import Citron.Names;
import :TypeId;

namespace Citron {

export class Identifier
{
    Name name;
    int typeParamCount;
    std::vector<TypeId> paramIds;

public:
    Identifier(Name&& name, int typeParamCount, std::vector<TypeId>&& paramIds);
    DECLARE_DEFAULTS(Identifier)
};

}