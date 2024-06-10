export module Citron.Symbols:MStructMemberVar;

import :MStruct;
import :MStructMemberVarDecl;
import :MSymbolComponent;

namespace Citron
{

export class MStructMemberVar : private MSymbolComponent<MStruct, MStructMemberVarDecl>
{

};


}