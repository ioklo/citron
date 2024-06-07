export module Citron.Symbols:MStructMemberFunc;

import <memory>;
import :MSymbolComponent;
import :MStruct;
import :MStructMemberFuncDecl;

namespace Citron
{

class MStructMemberFunc : private TMSymbolComponent<MStruct, MStructMemberFuncDecl>
{
};

}