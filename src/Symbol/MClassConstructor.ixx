export module Citron.Symbols:MClassConstructor;

import "std.h";
import :MClassConstructorDecl;
import :MSymbolComponent; 
import :MClass;

namespace Citron
{

export class MClassConstructor : private MSymbolComponent<MClass, MClassConstructorDecl>
{   
};

}