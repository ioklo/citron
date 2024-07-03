export module Citron.Symbols:MClassMemberFunc;

import "std.h";

import :MSymbolComponent;
import :MClass;
import :MClassMemberFuncDecl;

namespace Citron
{

export class MClassMemberFunc : private MSymbolComponent<MClass, MClassMemberFuncDecl>
{   
};

}