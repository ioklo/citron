#pragma once
#include <variant>
#include <span>
#include <vector>

namespace Citron {

// can handle both owning/nonowning span 

template<typename T>
struct USpan_Span 
{ 
    std::span<T> s;

    template<typename U>
    USpan_Span(std::vector<U>& v) : s{v} {};
};

template<typename T>
struct USpan_Vector { std::vector<T> v; };

template<typename T>
class USpan
{
    std::variant<std::span<T>, std::vector<T>> v;

public:
    using element_type = T;
    using value_type = std::remove_cv_t<T>;
    using iterator = T*;

    USpan(USpan_Span<T>&& uspan) : v{std::move(uspan.s)} {}
    USpan(USpan_Vector<T>&& uspan) : v{std::move(uspan.v)} {}

    T* data() { return std::visit([](auto& r) { return std::data(r); }, v); }
    std::size_t size() const { return std::visit([](auto const& r) { return std::size(r); }, v); }
    T* begin() { return data(); }
    T* end() { return data() + size(); }
    T& operator[](std::size_t i) { return data()[i]; }
};



} // namespace Citron