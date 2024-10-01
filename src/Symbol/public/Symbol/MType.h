#pragma once

#include <vector>
#include <memory>

#include "MFuncReturn.h"
#include "MFuncParameter.h"

namespace Citron 
{

class MNullableType;
class MTypeVarType;  // 이것은 Symbol인가?
class MVoidType;     // builtin type
class MTupleType;    // inline type
class MFuncType;     // inline type, circular
class MLocalPtrType; // inline type
class MBoxPtrType;   // inline type
class MInstanceType;
class MDeclId;
using MDeclIdPtr = std::shared_ptr<MDeclId>;

class MTypeArguments;
using MTypeArgumentsPtr = std::shared_ptr<MTypeArguments>;

class MTypeVisitor
{
public:
    virtual ~MTypeVisitor() { }
    virtual void Visit(MNullableType& type) = 0;
    virtual void Visit(MTypeVarType& type) = 0;
    virtual void Visit(MVoidType& type) = 0;
    virtual void Visit(MTupleType& type) = 0;
    virtual void Visit(MFuncType& type) = 0;
    virtual void Visit(MLocalPtrType& type) = 0;
    virtual void Visit(MBoxPtrType& type) = 0;
    virtual void Visit(MInstanceType& type) = 0;
};

class MType
{
public:
    virtual ~MType() { }
    virtual void Accept(MTypeVisitor& visitor) = 0;
};

using MTypePtr = std::shared_ptr<MType>;

// recursive types
class MNullableType : public MType
{
    MTypePtr innerType;

public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

// trivial types
// MyModule.MyClass<X, Y>.MyStruct<T, U, X>.T => 2 (Index는 누적)
// declId를 참조하게 만들지 않는 이유, FuncParamId 등을 만들기가 어렵다 (순환참조가 발생하기 쉽다)    
// public record class TypeVarSymbolId(int Index) : SymbolId;    
// => TypeVarSymbolId도 ModuleSymbolId의 일부분으로 통합한다. 사용할 때 resolution이 필요할거 같지만 큰 문제는 아닌 것 같다
// 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.X'
// => 순환참조때문에 누적 Index를 사용하는 TypeVarSymbolId로 다시 롤백한다
// 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.Func<T>(T, int).T' path에 Func<T>와 T가 순환 참조된다
// => TypeVarSymbolId(5)로 참조하게 한다
class MTypeVarType : public MType
{
    int index;
    // std::string name;

public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class MVoidType : public MType
{
public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class MTupleMemberVar
{
    MTypePtr declType;
    std::string name;
};

class MTupleType : public MType
{
    std::vector<MTupleMemberVar> memberVars;

public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class MFuncType : public MType
{
    bool bLocal;
    MFuncReturn funcRet;
    std::vector<MFuncParameter> parameters;

public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class MLocalPtrType : public MType
{
    MTypePtr innerType;

public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class MBoxPtrType : public MType
{
    MTypePtr innerType;
public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class MInstanceType : public MType
{
    MDeclIdPtr declId;
    MTypeArgumentsPtr typeArgs;

public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};


}

