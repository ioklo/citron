export module Citron.Ptr;

import <memory>;

namespace Citron {

export template<typename TType, typename... TArgs>
std::shared_ptr<TType> MakePtr(TArgs&&... args)
{
    return std::shared_ptr<TType>(new TType(std::forward<TArgs>(args)...));
}

}
