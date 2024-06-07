export module Citron.Symbols:MFunc;

import <variant>;
import :MGlobalFunc;

namespace Citron
{

export using MFunc = std::variant<MGlobalFunc>;


}