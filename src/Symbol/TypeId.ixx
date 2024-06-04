// internal, TypeId 선언만을 위해서 만든 헤더
export module Citron.Identifiers:TypeId;

import <variant>;
import <string>;
import <vector>;
import <memory>;
import Citron.Copy;

namespace Citron {

export using TypeId = std::variant<
    class TypeVarTypeId,
    class FixedNullableTypeId,
    class VoidTypeId,
    class TupleTypeId,
    class FixedFuncTypeId,
    class LambdaTypeId,
    class FixedLocalPtrTypeId,
    class FixedBoxPtrTypeId,
    class SymbolTypeId
>;

}