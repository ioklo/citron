export module Citron.Symbols:MBodyDeclOuter;

import <variant>;
import <memory>;

namespace Citron
{

export using MBodyDeclOuter = std::variant<
    std::weak_ptr<class MGlobalFuncDecl>,
    std::weak_ptr<class MClassConstructorDecl>,
    std::weak_ptr<class MClassMemberFuncDecl>,
    std::weak_ptr<class MStructConstructorDecl>,
    std::weak_ptr<class MStructMemberFuncDecl>,
    std::weak_ptr<class MLambdaDecl>
>;

}