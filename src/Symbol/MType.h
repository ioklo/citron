#pragma once
// forward declaration for MType

namespace Citron 
{
using MType = std::variant<
    std::unique_ptr<class MNullableType>,
    class MTypeVarType,  // 이것은 Symbol인가?
    class MVoidType,     // builtin type
    std::unique_ptr<class MTupleType>,    // inline type
    std::unique_ptr<class MFuncType>,     // inline type, circular
    std::unique_ptr<class MLocalPtrType>, // inline type
    std::unique_ptr<class MBoxPtrType>,   // inline type
    class MSymbolType
>;

class MTypeVarType
{
    int index;
    std::string name;
};

class MVoidType
{
};

class MSymbolType
{
};

}

