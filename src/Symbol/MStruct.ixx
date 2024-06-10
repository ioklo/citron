export module Citron.Symbols:MStruct;

import <vector>;
import :MStructDecl;
import :MSymbolComponent;
import :MTypeOuter;

namespace Citron
{

export class MStruct : private MSymbolComponent<MTypeOuter, MStructDecl>
{
};

}