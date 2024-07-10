#pragma once
// internal, TypeId 선언만을 위해서 만든 헤더
#include <memory>
#include <variant>

#include "Copy.h"

namespace Citron {

using MTypeId = std::variant<    
    std::unique_ptr<class MNullableTypeId>, 
    class MTypeVarTypeId,
    class MVoidTypeId,
    class MTupleTypeId,
    std::unique_ptr<class MFuncTypeId>,
    std::unique_ptr<class MLocalPtrTypeId>,
    std::unique_ptr<class MBoxPtrTypeId>,
    class MSymbolTypeId
>;

}