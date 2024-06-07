export module Citron.Symbols:MFunc;

import <variant>;
import :MGlobalFunc;

import :MClassConstructor;
import :MClassMemberFunc;

import :MStructConstructor;
import :MStructMemberFunc;

import :MLambda;

namespace Citron
{

export using MFunc = std::variant<
    MGlobalFunc,

    MClassConstructor,
    MClassMemberFunc,

    MStructConstructor,
    MStructMemberFunc,
    
    MLambda
>;


}