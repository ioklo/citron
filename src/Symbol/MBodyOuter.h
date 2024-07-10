#pragma once
#include <variant>
#include <memory>

namespace Citron
{

using MBodyOuter = std::variant<
    std::weak_ptr<class MGlobalFunc>,
    std::weak_ptr<class MClassConstructor>,
    std::weak_ptr<class MClassMemberFunc>,
    std::weak_ptr<class MStructConstructor>,
    std::weak_ptr<class MStructMemberFunc>,
    std::weak_ptr<class MLambda>
>;

}