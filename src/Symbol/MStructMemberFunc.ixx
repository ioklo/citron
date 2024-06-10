export module Citron.Symbols:MStructMemberFunc;

import <memory>;
import :MSymbolComponent;
import :MStruct;
import :MStructMemberFuncDecl;

namespace Citron
{

export class MStructMemberFunc : private MSymbolComponent<MStruct, MStructMemberFuncDecl>
{
};

}