module;
#include <expected>

export module Citron.Expected;

namespace Citron {

export template<typename V, typename E>
std::unexpected<E> Unexpected(std::expected<V, E>&& e)
{
    return std::unexpected{std::move(e).error()};
}

}
