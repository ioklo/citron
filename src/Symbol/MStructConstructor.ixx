export module Citron.Symbols:MStructConstructor;

import "std.h";

import :MStructConstructorDecl;
import :MSymbolComponent; 
import :MStruct;

namespace Citron
{

export class MStructConstructor : private MSymbolComponent<MStruct, MStructConstructorDecl>
{
};

}