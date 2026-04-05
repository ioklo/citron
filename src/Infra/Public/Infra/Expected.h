#pragma once

#include <expected>

namespace Citron {

template<typename V, typename E>
std::unexpected<E> Unexpected(std::expected<V, E>&& e)
{
    return std::unexpected{std::move(e).error()};
}

#define RETURN_ON_ERROR(e) do { if (!e) return std::unexpected{std::move(e).error()}; } while(0)
#define RETURN_ON_ERROR_REFDECL(e, ...) do { if (!e) return std::unexpected{std::move(e).error()}; } while(0); auto& __VA_ARGS__ = (*e)

}
