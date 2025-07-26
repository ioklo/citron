module;
#include "InfraConfig.h"
#include <cassert>
export module Citron.Unreachable;

namespace Citron {

export [[noreturn]] INFRA_API inline void unreachable() { assert(false); }

}