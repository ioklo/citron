export module Citron.Symbols:MClass;

import <vector>;
import :MClassDecl;
import :MSymbolComponent;
import :MTypeOuter;

namespace Citron
{

export class MClass : private MSymbolComponent<MTypeOuter, MClassDecl>
{   
};

}