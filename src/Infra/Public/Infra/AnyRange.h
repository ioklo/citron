#pragma once
#include <variant>
#include <functional>
#include <ranges>

namespace Citron {

template<typename T>
class AnyRange
{
    struct Generator
    {
        size_t count;
        std::function<T (size_t)> func;
    };

    using Variant = std::variant<std::span<T>, Generator>;
    
    Variant v;

    explicit AnyRange(Variant&& v) : v{std::move(v)}
    { }

public:
    static AnyRange FromSpan(std::span<T> s) { return AnyRange{std::move(s)}; }
    static AnyRange FromGenerator(size_t count, std::function<T (size_t)>&& func) { return AnyRange{Generator{count, std::move(func)}}; }

    size_t size() const
    {
        return std::visit([](auto& v) -> size_t {
            using T = std::remove_cvref_t<decltype(v)>;

            if constexpr (std::same_as<T, std::span<T>>)
                return v.size();
            else if constexpr (std::same_as<T, Generator>)
                return v.count;
            else static_assert(false);

        }, v);
    }

    T Get(size_t i) const
    {
        return std::visit([i](auto& v) -> T {
            using T = std::remove_cvref_t<decltype(v)>;
            if constexpr (std::same_as<T, std::span<T>>) return v[i];
            else if constexpr (std::same_as<T, Generator>) return v.func(i);
            else static_assert(false);
        }, v);
    }

    struct Cursor
    {
        const AnyRange* range;
        size_t index;

        T operator*() const
        {
            return range->Get(index);
        }

        Cursor& operator++()
        {
            ++index;
            return *this;
        }

        bool operator!=(Cursor const& other) const
        {
            return index != other.index;
        }
    };

    Cursor begin() const
    {
        return Cursor{this, 0};
    }

    Cursor end() const
    {
        return Cursor{this, size()};
    }
};

} // namespace Citron
