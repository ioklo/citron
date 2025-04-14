export module Citron.Expected;

import <expected>;

namespace Citron {

export template<typename V, typename E>
std::unexpected<E> Unexpected(std::expected<V, E>&& e)
{
    return std::unexpected{std::move(e).error()};
}

}
