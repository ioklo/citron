export module Citron.Symbols:MClassMemberFunc;

import <memory>;
import :MSymbolComponent;
import :MClass;
import :MClassMemberFuncDecl;

namespace Citron
{

class MClassMemberFunc : private TMSymbolComponent<MClass, MClassMemberFuncDecl>
{   
};

}