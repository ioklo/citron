export module Citron.Unreachable;

import "InfraConfig.h";
import <cassert>;

namespace Citron {

export [[noreturn]] inline void unreachable() { assert(false); }

}