#pragma once
#include "SymbolMacros.h"

// TypeId의 실제 implementation, TypeId를 complete type으로 쓰고 싶으면 이 모듈을 import해야 한다

#include "MTypeId.h"
#include "MIdentifier.h"
#include "MSymbolId.h"

// TypeId가 멤버로 들어가는 TypeId를 선언

namespace Citron {

class MNullableTypeId;
class MTupleMemberVarId;
class MFuncTypeId;
class MLocalPtrTypeId;
class MBoxPtrTypeId;

// MyModule.MyClass<X, Y>.MyStruct<T, U, X>.T => 2 (Index는 누적)
// declId를 참조하게 만들지 않는 이유, FuncParamId 등을 만들기가 어렵다 (순환참조가 발생하기 쉽다)    
// public record class TypeVarSymbolId(int Index) : SymbolId;    
// => TypeVarSymbolId도 ModuleSymbolId의 일부분으로 통합한다. 사용할 때 resolution이 필요할거 같지만 큰 문제는 아닌 것 같다
// 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.X'
// => 순환참조때문에 누적 Index를 사용하는 TypeVarSymbolId로 다시 롤백한다
// 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.Func<T>(T, int).T' path에 Func<T>와 T가 순환 참조된다
// => TypeVarSymbolId(5)로 참조하게 한다
class MTypeVarTypeId
{
    int index;
    std::string name;

public:
    // MTypeVarTypeId(int index, std::string name);
    // DECLARE_DEFAULTS(MTypeVarTypeId)

    int GetIndex() { return index; }
    const std::string& GetName() { return name; }
};

class MVoidTypeId
{
public:
    MVoidTypeId() { }
    DECLARE_DEFAULTS(MVoidTypeId)
};

class MTupleTypeId
{
    std::vector<MTupleMemberVarId> memberVarIds;

public:
    DECLARE_DEFAULTS(MTupleTypeId)
    auto GetMemberVarIds();
};

class MLambdaTypeId
{
public:
    DECLARE_DEFAULTS(MLambdaTypeId)
};

class MSymbolTypeId
{
    bool bLocal;
    MSymbolId symbolId;

public:
    SYMBOL_API MSymbolTypeId(bool bLocal, MSymbolId&& symbolId);
    DECLARE_DEFAULTS(MSymbolTypeId)
};

// 여기부터 TypeId가 완전해진다
class MNullableTypeId
{
    MTypeId innerTypeId;

public:
    MNullableTypeId(MTypeId innerTypeId);
    DECLARE_DEFAULTS(MNullableTypeId)

    const MTypeId& GetInnerTypeId();

};

class MTupleMemberVarId
{
    MTypeId typeId;
    MIdentifier varId;

public:
    MTupleMemberVarId(MTypeId&& typeId, MIdentifier&& varId);
};

class MFuncTypeId
{
    bool bLocal;
    MTypeId retId;
    std::vector<MTypeId> paramIds;

public:
    DECLARE_DEFAULTS(MFuncTypeId)

    bool IsLocal();
    const MTypeId& GetReturn();
};

class MLocalPtrTypeId
{
    MTypeId innerType;

public:
    DECLARE_DEFAULTS(MLocalPtrTypeId)
};

class MBoxPtrTypeId
{
    MTypeId innerType;

public:
    DECLARE_DEFAULTS(MBoxPtrTypeId)
};

//struct MTypeIdCopyVisitor
//{
//    template<typename T>
//    MTypeId operator()(T&& typeId) { return MTypeId(typeId.Copy()); }
//
//    /*template<typename T>
//    MTypeId operator()(std::unique_ptr<T>& typeId) { return MTypeId(make_unique(typeId->Copy())); }*/
//};

MTypeId Copy(const MTypeId& typeId);
//{   
//    return std::visit(MTypeIdCopyVisitor(), typeId);
//}

extern MVoidTypeId voidTypeId;
extern MTypeId boolTypeId;
extern MTypeId intTypeId;
extern MTypeId stringTypeId;

}