#pragma once

namespace Citron {

template<typename T>
class View
{
    using GetterFn = T (*)(void* context, size_t i) noexcept;

    void* context;
    size_t n;
    GetterFn getter;

public:
    View(void* context, size_t n, GetterFn getter) noexcept
        : context{context}, n{n}, getter{getter}
    { }

    size_t size() const noexcept { return n; }
    T operator[](size_t i) const noexcept { return getter(context, i); }

    struct iterator {
        const View<T>* r;
        size_t i;
        T operator*() const noexcept { return (*r)[i]; }
        iterator& operator++() noexcept { ++i; return *this; }
        bool operator!=(const iterator& o) const noexcept { return i != o.i; }
    };

    iterator begin() const noexcept { return {this, 0}; }
    iterator end()   const noexcept { return {this, n}; }
};


} // Citron
