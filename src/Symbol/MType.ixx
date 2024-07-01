// forward declaration for MType
export module Citron.Symbols:MType;

import <variant>;
import <memory>;
import <string>;

namespace Citron 
{
export using MType = std::variant<
    std::unique_ptr<class MNullableType>,
    class MTypeVarType,  // 이것은 Symbol인가?
    class MVoidType,     // builtin type
    std::unique_ptr<class MTupleType>,    // inline type
    std::unique_ptr<class MFuncType>,     // inline type, circular
    std::unique_ptr<class MLocalPtrType>, // inline type
    std::unique_ptr<class MBoxPtrType>,   // inline type
    class MSymbolType
>;

export class MTypeVarType
{
    int index;
    std::string name;
};

export class MVoidType
{
};

export class MSymbolType
{
};

}

