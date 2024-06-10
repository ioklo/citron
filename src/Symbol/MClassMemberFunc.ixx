export module Citron.Symbols:MClassMemberFunc;

import <memory>;
import :MSymbolComponent;
import :MClass;
import :MClassMemberFuncDecl;

namespace Citron
{

export class MClassMemberFunc : private MSymbolComponent<MClass, MClassMemberFuncDecl>
{   
};

}