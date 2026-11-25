#pragma once
#include <memory>
#include "Concepts.h"

namespace Citron {

template<UnqualifiedType T>
class InRefPtr
{
    std::shared_ptr<T>& ref;

public:
    InRefPtr(std::shared_ptr<T>& ref) noexcept
        : ref{ref}
    {
    }

    InRefPtr(std::shared_ptr<T>&& value) noexcept
        : ref{value} // 이름이 생긴 T&&는 lvalue
    {
    }

    T& operator*() const noexcept { return *ref; }
    T* operator->() const noexcept { return ref.get(); }

    std::shared_ptr<T> Share() const noexcept { return ref; }
};

} // namespace Citron