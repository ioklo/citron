export module Citron.Symbols:MStructConstructor;

import <memory>;
import :MStructConstructorDecl;
import :MSymbolComponent; 
import :MStruct;

namespace Citron
{

class MStructConstructor : private TMSymbolComponent<MStruct, MStructConstructorDecl>
{
};

}