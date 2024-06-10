export module Citron.Symbols:MEnumElem;

import :MSymbolComponent;
import :MEnumElemDecl;

namespace Citron
{

class MEnum;

export class MEnumElem : private MSymbolComponent<std::shared_ptr<MEnum>, MEnumElemDecl>
{

};

}