#pragma once
// internal, TypeId 선언만을 위해서 만든 헤더
#include <memory>
#include <variant>

#include "Copy.h"
#include "MIdentifier.h"
#include "MSymbolId.h"

namespace Citron {

class MNullableTypeId;
class MTypeVarTypeId;
class MVoidTypeId;
class MTupleTypeId;
class MFuncTypeId;
class MLambdaTypeId;
class MLocalPtrTypeId;
class MBoxPtrTypeId;
class MSymbolTypeId;

class MTypeIdVisitor
{
public:
    virtual ~MTypeIdVisitor() { }
    virtual void Visit(MNullableTypeId& typeId) = 0;
    virtual void Visit(MTypeVarTypeId& typeId) = 0;
    virtual void Visit(MVoidTypeId& typeId) = 0;
    virtual void Visit(MTupleTypeId& typeId) = 0;
    virtual void Visit(MFuncTypeId& typeId) = 0;
    virtual void Visit(MLambdaTypeId& typeId) = 0;
    virtual void Visit(MLocalPtrTypeId& typeId) = 0;
    virtual void Visit(MBoxPtrTypeId& typeId) = 0;
    virtual void Visit(MSymbolTypeId& typeId) = 0;
};

class MTypeId
{
public:
    virtual ~MTypeId() { }
    virtual void Accept(MTypeIdVisitor& visitor) = 0;
};

using MTypeIdPtr = std::shared_ptr<MTypeId>;

class MNullableTypeId : public MTypeId
{
    std::shared_ptr<MTypeId> innerTypeId;

public:
    MNullableTypeId(std::shared_ptr<MTypeId> innerTypeId);
    DECLARE_DEFAULTS(MNullableTypeId)

    const MTypeId& GetInnerTypeId();
    void Accept(MTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

// MyModule.MyClass<X, Y>.MyStruct<T, U, X>.T => 2 (Index는 누적)
// declId를 참조하게 만들지 않는 이유, FuncParamId 등을 만들기가 어렵다 (순환참조가 발생하기 쉽다)    
// public record class TypeVarSymbolId(int Index) : SymbolId;    
// => TypeVarSymbolId도 ModuleSymbolId의 일부분으로 통합한다. 사용할 때 resolution이 필요할거 같지만 큰 문제는 아닌 것 같다
// 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.X'
// => 순환참조때문에 누적 Index를 사용하는 TypeVarSymbolId로 다시 롤백한다
// 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.Func<T>(T, int).T' path에 Func<T>와 T가 순환 참조된다
// => TypeVarSymbolId(5)로 참조하게 한다
class MTypeVarTypeId : public MTypeId
{
    int index;
    std::string name;

public:
    // MTypeVarTypeId(int index, std::string name);
    // DECLARE_DEFAULTS(MTypeVarTypeId)

    int GetIndex() { return index; }
    const std::string& GetName() { return name; }

    void Accept(MTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class MVoidTypeId : public MTypeId
{
public:
    MVoidTypeId() { }
    DECLARE_DEFAULTS(MVoidTypeId)
    void Accept(MTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class MTupleMemberVarId
{
    std::shared_ptr<MTypeId> typeId;
    MIdentifier varId;

public:
    MTupleMemberVarId(std::shared_ptr<MTypeId>&& typeId, MIdentifier&& varId);
};

class MTupleTypeId : public MTypeId
{
    std::vector<MTupleMemberVarId> memberVarIds;

public:
    DECLARE_DEFAULTS(MTupleTypeId)
    auto GetMemberVarIds();
    void Accept(MTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class MFuncTypeId : public MTypeId
{
    bool bLocal;
    std::shared_ptr<MTypeId> retId;
    std::vector<std::shared_ptr<MTypeId>> paramIds;

public:
    DECLARE_DEFAULTS(MFuncTypeId)

    bool IsLocal();
    const MTypeId& GetReturn();
    void Accept(MTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class MLambdaTypeId : public MTypeId
{
public:
    DECLARE_DEFAULTS(MLambdaTypeId)
    void Accept(MTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class MLocalPtrTypeId : public MTypeId
{
    std::shared_ptr<MTypeId> innerType;

public:
    DECLARE_DEFAULTS(MLocalPtrTypeId)
    void Accept(MTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class MBoxPtrTypeId : public MTypeId
{
    std::shared_ptr<MTypeId> innerType;

public:
    DECLARE_DEFAULTS(MBoxPtrTypeId)
    void Accept(MTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class MSymbolTypeId : public MTypeId
{
    bool bLocal;
    MSymbolId symbolId;

public:
    SYMBOL_API MSymbolTypeId(bool bLocal, MSymbolId&& symbolId);
    DECLARE_DEFAULTS(MSymbolTypeId)
    void Accept(MTypeIdVisitor& visitor) override { visitor.Visit(*this); }
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
extern std::shared_ptr<MTypeId> boolTypeId;
extern std::shared_ptr<MTypeId> intTypeId;
extern std::shared_ptr<MTypeId> stringTypeId;

}