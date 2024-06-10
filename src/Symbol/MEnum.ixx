export module Citron.Symbols:MEnum;

import <variant>;
import <memory>;
import :MEnumDecl;
import :MSymbolComponent;
import :MTypeOuter;

namespace Citron
{

export class MEnum : private MSymbolComponent<MTypeOuter, MEnumDecl>
{

};

}