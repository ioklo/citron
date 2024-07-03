export module Citron.Symbols:MEnumElem;

import "std.h";
import :MSymbolComponent;
import :MEnumElemDecl;

namespace Citron
{

export class MEnum;

export class MEnumElem : private MSymbolComponent<std::shared_ptr<MEnum>, MEnumElemDecl>
{

};

}