export module Citron.Symbols:MStructMemberFunc;

import "std.h";

import :MSymbolComponent;
import :MStruct;
import :MStructMemberFuncDecl;

namespace Citron
{

export class MStructMemberFunc : private MSymbolComponent<MStruct, MStructMemberFuncDecl>
{
};

}