#pragma once

#include <expected>

namespace Citron {

template<typename V, typename E>
std::unexpected<E> Unexpected(std::expected<V, E>&& e)
{
    return std::unexpected{std::move(e).error()};
}

#define RETURN_ON_ERROR(e) { if (!e) return std::unexpected{std::move(e).error()}; }

}
