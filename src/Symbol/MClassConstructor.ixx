export module Citron.Symbols:MClassConstructor;

import <memory>;
import :MClassConstructorDecl;
import :MSymbolComponent; 
import :MClass;

namespace Citron
{

export class MClassConstructor : private MSymbolComponent<MClass, MClassConstructorDecl>
{   
};

}