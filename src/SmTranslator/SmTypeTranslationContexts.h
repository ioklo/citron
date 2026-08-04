#pragma once
#include "SmTypeResolveScope.h"

namespace Citron {

class RFactory;

struct SmTypeTranslationContexts
{
    SmTypeResolveScope scope;
    RFactory* rFactory;
};


} // namespace Citron
