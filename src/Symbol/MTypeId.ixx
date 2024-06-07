// internal, TypeId 선언만을 위해서 만든 헤더
export module Citron.Symbols:MTypeId;

import <variant>;
import <string>;
import <vector>;
import <memory>;
import Citron.Copy;

namespace Citron {

export using MTypeId = std::variant<    
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