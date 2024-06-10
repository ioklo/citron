export module Citron.Symbols:MClassMemberVar;

import :MClass;
import :MClassMemberVarDecl;
import :MSymbolComponent;

namespace Citron
{

export class MClassMemberVar : private MSymbolComponent<MClass, MClassMemberVarDecl>
{

};


}