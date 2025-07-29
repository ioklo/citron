#pragma once
#include "InfraConfig.h"

#include <cassert>

namespace Citron {

[[noreturn]] INFRA_API inline void unreachable() { assert(false); }

}