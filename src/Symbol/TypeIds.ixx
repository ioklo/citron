module;
#include "Macros.h"

// TypeId의 실제 implementation, TypeId를 complete type으로 쓰고 싶으면 이 모듈을 import해야 한다
export module Citron.Identifiers:TypeIds;

import <string>;
import <memory>;
import <vector>;
import <ranges>;
import :TypeId;
import :Identifier;
import :SymbolId;

// TypeId가 멤버로 들어가는 TypeId를 선언

namespace Citron {

class NullableTypeId;
class TupleMemberVarId;
class FuncTypeId;
class LocalPtrTypeId;
class BoxPtrTypeId;

// MyModule.MyClass<X, Y>.MyStruct<T, U, X>.T => 2 (Index는 누적)
// declId를 참조하게 만들지 않는 이유, FuncParamId 등을 만들기가 어렵다 (순환참조가 발생하기 쉽다)    
// public record class TypeVarSymbolId(int Index) : SymbolId;    
// => TypeVarSymbolId도 ModuleSymbolId의 일부분으로 통합한다. 사용할 때 resolution이 필요할거 같지만 큰 문제는 아닌 것 같다
// 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.X'
// => 순환참조때문에 누적 Index를 사용하는 TypeVarSymbolId로 다시 롤백한다
// 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.Func<T>(T, int).T' path에 Func<T>와 T가 순환 참조된다
// => TypeVarSymbolId(5)로 참조하게 한다
export class TypeVarTypeId
{
    int index;
    std::string name;

public:
    TypeVarTypeId(int index, std::string name);
    DECLARE_DEFAULTS(TypeVarTypeId)

    int GetIndex() { return index; }
    const std::string& GetName() { return name; }
};

export class FixedNullableTypeId
{
    std::unique_ptr<NullableTypeId> impl;

public:
    FixedNullableTypeId(TypeId&& innerTypeId);
    DECLARE_DEFAULTS(FixedNullableTypeId)

    const TypeId& GetInnerTypeId();
    
};

export class VoidTypeId
{
public:
    VoidTypeId();
    DECLARE_DEFAULTS(VoidTypeId)
};

export class TupleTypeId
{
    std::vector<TupleMemberVarId> memberVarIds;

public:
    DECLARE_DEFAULTS(TupleTypeId)
    auto GetMemberVarIds();
};

export class FixedFuncTypeId
{
    std::unique_ptr<FuncTypeId> impl;

public:
    DECLARE_DEFAULTS(FixedFuncTypeId)

    bool IsLocal();
    const TypeId& GetReturn();
    auto GetParams();
};

export class LambdaTypeId
{
public:
    DECLARE_DEFAULTS(LambdaTypeId)
};

export class FixedLocalPtrTypeId
{
    std::unique_ptr<LocalPtrTypeId> impl;

public:
    DECLARE_DEFAULTS(FixedLocalPtrTypeId)
};

export class FixedBoxPtrTypeId
{
    std::unique_ptr<BoxPtrTypeId> impl;

public:
    DECLARE_DEFAULTS(FixedBoxPtrTypeId)
};

export class SymbolTypeId
{
    bool bLocal;
    SymbolId symbolId;

public:
    SymbolTypeId(bool bLocal, SymbolId&& symbolId);
    DECLARE_DEFAULTS(SymbolTypeId)
};

// 여기부터 TypeId가 완전해진다
export class NullableTypeId
{
    TypeId innerTypeId;

public:
    NullableTypeId(TypeId innerTypeId);
    DECLARE_DEFAULTS(NullableTypeId)

    const TypeId& GetInnerTypeId();

};

export class TupleMemberVarId
{
    TypeId typeId;
    Identifier varId;

public:
    TupleMemberVarId(TypeId&& typeId, Identifier&& varId);
};

export class FuncTypeId
{
    bool bLocal;
    TypeId retId;
    std::vector<TypeId> paramIds;

public:
    DECLARE_DEFAULTS(FuncTypeId)

    bool IsLocal();
    const TypeId& GetReturn();
};

export class LocalPtrTypeId
{
    TypeId innerType;

public:
    DECLARE_DEFAULTS(LocalPtrTypeId)
};

export class BoxPtrTypeId
{
    TypeId innerType;

public:
    DECLARE_DEFAULTS(BoxPtrTypeId)
};

export TypeId Copy(const TypeId& typeId)
{
    return std::visit([](auto&& typeId) { return TypeId(typeId.Copy()); }, typeId);
}

export extern VoidTypeId voidTypeId;
export extern TypeId boolTypeId;
export extern TypeId intTypeId;
export extern TypeId stringTypeId;

}