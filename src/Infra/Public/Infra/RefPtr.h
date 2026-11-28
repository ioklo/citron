#pragma once
#include <memory>
#include <cassert>
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
    T* GetRawPtr() const noexcept { return ref.get(); }
};

// TakeRef<std::shared_ptr<T>> 대신에 쓰는 
template<typename T> requires std::same_as<T, std::remove_cvref_t<T>>
class TakeRefPtr
{
    std::shared_ptr<T>* ptr; // null일 경우 moved
    bool movable;

public:
    TakeRefPtr(std::shared_ptr<T>& t) : ptr{&t}, movable(false) {}
    TakeRefPtr(std::shared_ptr<T>&& t) : ptr{&t}, movable(true) {}
    TakeRefPtr(const TakeRefPtr&) = delete;
    TakeRefPtr(TakeRefPtr&& other)
        : ptr{other.ptr}, movable{other.movable}
    {
        other.ptr = nullptr;
        other.movable = false;
    }

    TakeRefPtr& operator=(const TakeRefPtr&) = delete;
    TakeRefPtr& operator=(TakeRefPtr&& other)
    {
        if (this == &other) return *this;

        this->ptr = other.ptr;
        this->movable = other.movable;

        other.ptr = nullptr;
        other.movable = false;
        return *this;
    }
    T& operator*() noexcept { return **ptr; }
    T* operator->() noexcept { return ptr->get(); }

    bool IsMovable() noexcept { return movable; }

    std::shared_ptr<T>&& Move()
    {
        assert(movable);
        return std::move(*ptr);
    }

    std::shared_ptr<T> Take()
    {
        if (movable)
            return std::move(*ptr);
        else
            return *ptr;
    }

    template<typename U>
    void AssignTo(U&& u)
    {
        if (movable)
            u = std::move(*ptr);
        else
            u = *ptr;
    }
};

} // namespace Citron