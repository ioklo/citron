export module Citron.Symbols:MClassMemberVar;

import :MClass;
import :MClassMemberVarDecl;
import :MSymbolComponent;

namespace Citron
{

class MClassMemberVar : private TMSymbolComponent<MClass, MClassMemberVarDecl>
{

};


}