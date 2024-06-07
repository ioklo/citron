export module Citron.Symbols:MStructMemberVar;

import :MStruct;
import :MStructMemberVarDecl;
import :MSymbolComponent;

namespace Citron
{

class MStructMemberVar : private TMSymbolComponent<MStruct, MStructMemberVarDecl>
{

};


}