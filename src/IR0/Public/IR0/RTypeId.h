#pragma once
// internal, TypeId 선언만을 위해서 만든 헤더
#include <memory>
#include <variant>

#include <Infra/Defaults.h>
#include <Infra/Copy.h>

#include "RIdentifier.h"
#include "RSymbolId.h"

namespace Citron {

class RNullableTypeId;
class RTypeVarTypeId;
class RVoidTypeId;
class RTupleTypeId;
class RFuncTypeId;
class RLambdaTypeId;
class RLocalPtrTypeId;
class RBoxPtrTypeId;
class RSymbolTypeId;

class RTypeIdVisitor
{
public:
    virtual ~RTypeIdVisitor() { }
    virtual void Visit(RNullableTypeId& typeId) = 0;
    virtual void Visit(RTypeVarTypeId& typeId) = 0;
    virtual void Visit(RVoidTypeId& typeId) = 0;
    virtual void Visit(RTupleTypeId& typeId) = 0;
    virtual void Visit(RFuncTypeId& typeId) = 0;
    virtual void Visit(RLambdaTypeId& typeId) = 0;
    virtual void Visit(RLocalPtrTypeId& typeId) = 0;
    virtual void Visit(RBoxPtrTypeId& typeId) = 0;
    virtual void Visit(RSymbolTypeId& typeId) = 0;
};

class RTypeId
{
public:
    virtual ~RTypeId() { }
    virtual void Accept(RTypeIdVisitor& visitor) = 0;
};

using RTypeIdPtr = std::shared_ptr<RTypeId>;

class RNullableTypeId : public RTypeId
{
    std::shared_ptr<RTypeId> innerTypeId;

public:
    RNullableTypeId(std::shared_ptr<RTypeId> innerTypeId);
    DECLARE_DEFAULTS(IR0_API, RNullableTypeId)

    const RTypeId& GetInnerTypeId();
    void Accept(RTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

// MyModule.MyClass<X, Y>.MyStruct<T, U, X>.T => 2 (Index는 누적)
// declId를 참조하게 만들지 않는 이유, FuncParamId 등을 만들기가 어렵다 (순환참조가 발생하기 쉽다)    
// public record class TypeVarSymbolId(int Index) : SymbolId;    
// => TypeVarSymbolId도 ModuleSymbolId의 일부분으로 통합한다. 사용할 때 resolution이 필요할거 같지만 큰 문제는 아닌 것 같다
// 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.X'
// => 순환참조때문에 누적 Index를 사용하는 TypeVarSymbolId로 다시 롤백한다
// 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.Func<T>(T, int).T' path에 Func<T>와 T가 순환 참조된다
// => TypeVarSymbolId(5)로 참조하게 한다
class RTypeVarTypeId : public RTypeId
{
    int index;
    std::string name;

public:
    // RTypeVarTypeId(int index, std::string name);
    // DECLARE_DEFAULTS(RTypeVarTypeId)

    int GetIndex() { return index; }
    const std::string& GetName() { return name; }

    void Accept(RTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class RVoidTypeId : public RTypeId
{
public:
    RVoidTypeId() { }
    DECLARE_DEFAULTS(IR0_API, RVoidTypeId)
    void Accept(RTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class RTupleMemberVarId
{
    std::shared_ptr<RTypeId> typeId;
    RIdentifier varId;

public:
    RTupleMemberVarId(std::shared_ptr<RTypeId>&& typeId, RIdentifier&& varId);
};

class RTupleTypeId : public RTypeId
{
    std::vector<RTupleMemberVarId> memberVarIds;

public:
    DECLARE_DEFAULTS(IR0_API, RTupleTypeId)
    auto GetMemberVarIds();
    void Accept(RTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class RFuncTypeId : public RTypeId
{
    bool bLocal;
    std::shared_ptr<RTypeId> retId;
    std::vector<std::shared_ptr<RTypeId>> paramIds;

public:
    DECLARE_DEFAULTS(IR0_API, RFuncTypeId)

    bool IsLocal();
    const RTypeId& GetReturn();
    void Accept(RTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class RLambdaTypeId : public RTypeId
{
public:
    DECLARE_DEFAULTS(IR0_API, RLambdaTypeId)
    void Accept(RTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class RLocalPtrTypeId : public RTypeId
{
    std::shared_ptr<RTypeId> innerType;

public:
    DECLARE_DEFAULTS(IR0_API, RLocalPtrTypeId)
    void Accept(RTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class RBoxPtrTypeId : public RTypeId
{
    std::shared_ptr<RTypeId> innerType;

public:
    DECLARE_DEFAULTS(IR0_API, RBoxPtrTypeId)
    void Accept(RTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

class RSymbolTypeId : public RTypeId
{
    bool bLocal;
    RSymbolId symbolId;

public:
    IR0_API RSymbolTypeId(bool bLocal, RSymbolId&& symbolId);
    DECLARE_DEFAULTS(IR0_API, RSymbolTypeId)
    void Accept(RTypeIdVisitor& visitor) override { visitor.Visit(*this); }
};

RTypeId Copy(const RTypeId& typeId);

extern RVoidTypeId voidTypeId;
extern std::shared_ptr<RTypeId> boolTypeId;
extern std::shared_ptr<RTypeId> intTypeId;
extern std::shared_ptr<RTypeId> stringTypeId;

}