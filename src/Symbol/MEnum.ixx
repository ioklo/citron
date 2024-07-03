export module Citron.Symbols:MEnum;

import "std.h";

import :MEnumDecl;
import :MSymbolComponent;
import :MTypeOuter;

namespace Citron
{

export class MEnum : private MSymbolComponent<MTypeOuter, MEnumDecl>
{

};

}