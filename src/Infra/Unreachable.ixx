export module Citron.Unreachable;

import "InfraConfig.h";
import <cassert>;

namespace Citron {

export [[noreturn]] INFRA_API inline void unreachable() { assert(false); }

}