#pragma once

#include <memory>

namespace Citron {

template<typename TType, typename... TArgs>
std::shared_ptr<TType> MakePtr(TArgs&&... args)
{
    return std::shared_ptr<TType>(new TType(std::forward<TArgs>(args)...));
}

}
