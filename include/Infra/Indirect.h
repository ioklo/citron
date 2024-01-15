#pragma once
#include "InfraConfig.h"
#include <memory>
#include <utility>

namespace Citron::Infra {

template<typename T>
class Indirect
{
    std::unique_ptr<T> ptr;

public:
    Indirect(T* t)
        : ptr(t)
    {
    }

    Indirect(const Indirect<T>& other)
        : ptr(std::make_unique<T>(*other.ptr))
    {
    }

    Indirect(Indirect<T>&& other)
        : ptr(std::move(other.ptr))
    {
    }

    Indirect& operator=(const Indirect<T>& other)
    {
        ptr = std::make_unique<T>(*other.ptr);
        return *this;
    }

    Indirect& operator=(Indirect<T>&& other)
    {
        ptr = std::move(other.ptr);
        return *this;
    }
};

}