#pragma once
#include "InfraConfig.h"
#include <cassert>

namespace Citron {

[[noreturn]] inline void unreachable() { assert(false); }

}