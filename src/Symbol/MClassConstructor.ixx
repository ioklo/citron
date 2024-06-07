export module Citron.Symbols:MClassConstructor;

import <memory>;
import :MClassConstructorDecl;
import :MSymbolComponent; 
import :MClass;

namespace Citron
{

class MClassConstructor : private TMSymbolComponent<MClass, MClassConstructorDecl>
{   
};

}