#pragma once
#include <variant>
#include <concepts>
#include <cassert>

namespace Citron {

template<class T>
concept UnqualifiedType = std::same_as<T, std::remove_cvref_t<T>>;

template<UnqualifiedType T>
class InRef
{
    std::reference_wrapper<T> ref;

public:
    InRef(T& value) noexcept
        : ref{value}
    {
    }

    InRef(T&& value) noexcept
        : ref{value} // 이름이 생긴 T&&는 lvalue
    {
    }

    T& operator*() const noexcept { return ref.get(); }
    T* operator->() const noexcept { return &ref.get(); }
};


// 함수 인자로 쓰며, 함수 내부에서 저장할 용도로 쓴다. lvalue/rvalue 레퍼런스 둘다 받을수 있다
// 1. TakeRef<T>로 받은 것을 내부에서 assign => AssignTo
// 2. TakeRef<T>로 받은 것을 다른 함수의 TakeRef<T>로 forward => std::move로 전달
// 3. TakeRef<T>로 받은 것을 다른 함수의 T로 전달 => Take
// 4. TakeRef<T>로 받은 것을 다른 함수의 T&로 전달 => operator*
// 5. TakeRef<T>로 받은 것을 다른 함수의 T&&로 전달 => rvalue 검사후, move or Take
template<typename T> requires std::same_as<T, std::remove_cvref_t<T>>
class TakeRef
{
    T* ptr; // null일 경우 moved
    bool movable;

public:
    TakeRef(T& t) : ptr{&t}, movable(false) {}
    TakeRef(T&& t) : ptr{&t}, movable(true) {}
    TakeRef(const TakeRef&) = delete;
    TakeRef(TakeRef&& other)
        : ptr{other.ptr}, movable{other.movable}
    {
        other.ptr = nullptr;
        other.movable = false;
    }

    TakeRef& operator=(const TakeRef&) = delete;
    TakeRef& operator=(TakeRef&& other)
    {
        this->ptr = other.ptr;
        this->movable = other.movable;

        other.ptr = nullptr;
        other.movable = false;
        return *this;
    }
    T& operator*() noexcept { return *ptr; }
    T* operator->() noexcept { return ptr; }

    bool IsMovable() noexcept { return movable; }

    T&& Move()
    {
        assert(movable);
        return std::move(*ptr);
    }

    T Take()
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

template<UnqualifiedType T>
class RefOrOwn
{
    using Storage = std::variant<std::reference_wrapper<T>, T>;
    Storage storage;

public:
    RefOrOwn(T& value) noexcept
        : storage(std::ref(value))
    {
    }

    RefOrOwn(T&& value) noexcept(std::is_nothrow_move_constructible_v<T>)
        : storage{std::in_place_type<T>, std::move(value)}
    {
    }

    bool IsOwned() noexcept
    {
        return std::holds_alternative<T>(storage);
    }

    T& Get() noexcept
    {
        return visit([](auto& value) -> T& { 
            using U = std::remove_cvref_t<decltype(value)>;
            if constexpr (std::same_as<U, std::reference_wrapper<T>>) return value.get();
            else if constexpr (std::same_as<U, T>) return value;
            else static_assert(false);
        }, storage);
    }
    
    T& operator*() noexcept { return Get(); }
    T* operator->() noexcept { return &Get(); }
};

} // namespace Citron