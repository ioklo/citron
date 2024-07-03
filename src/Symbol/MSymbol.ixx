export module Citron.Symbols:MSymbol;

import "std.h";

namespace Citron
{

using MSymbol = std::variant<
    std::shared_ptr<class MGlobalFunc>,

    std::shared_ptr<class MClass>,
    std::shared_ptr<class MClassConstructor>,
    std::shared_ptr<class MClassMemberFunc>,
    std::shared_ptr<class MClassMemberVar>,

    std::shared_ptr<class MStruct>,
    std::shared_ptr<class MStructConstructor>,
    std::shared_ptr<class MStructMemberFunc>,
    std::shared_ptr<class MStructMemberVar>,

    std::shared_ptr<class MEnum>,
    std::shared_ptr<class MEnumElem>,
    std::shared_ptr<class MEnumElemMemberVar>,

    std::shared_ptr<class MLambda>,
    std::shared_ptr<class MLambdaMemberVar>,

    std::shared_ptr<class MInterface>
>;

}