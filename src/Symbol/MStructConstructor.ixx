export module Citron.Symbols:MStructConstructor;

import <memory>;
import :MStructConstructorDecl;
import :MSymbolComponent; 
import :MStruct;

namespace Citron
{

export class MStructConstructor : private MSymbolComponent<MStruct, MStructConstructorDecl>
{
};

}