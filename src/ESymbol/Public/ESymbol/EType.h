#pragma once

#include <vector>
#include <string>

#include "EFuncReturn.h"
#include "EFuncParameter.h"

namespace Citron
{

class EDeclId;

class ETypeArguments;

// 같은 unit내에서의 forward declaration
class EType_Nullable;
class EType_TypeVar;  // 이것은 Symbol인가?
class EType_Void;     // builtin type
class EType_Tuple;    // inline type
class EType_Func;     // inline type, circular
class EType_Ptr; // inline type
class EType_Box;   // inline type
class EType_Instance;

class ETypeVisitor
{
public:
    virtual ~ETypeVisitor() {}
    virtual void Visit(EType_Nullable* type) = 0;
    virtual void Visit(EType_TypeVar* type) = 0;
    virtual void Visit(EType_Void* type) = 0;
    virtual void Visit(EType_Tuple* type) = 0;
    virtual void Visit(EType_Func* type) = 0;
    virtual void Visit(EType_Ptr* type) = 0;
    virtual void Visit(EType_Box* type) = 0;
    virtual void Visit(EType_Instance* type) = 0;
};

class EType
{
public:
    virtual ~EType() {}
    virtual void Accept(ETypeVisitor& visitor) = 0;
};

// recursive types
class EType_Nullable : public EType
{
    EType* innerType;

public:
    void Accept(ETypeVisitor& visitor) override { visitor.Visit(this); }
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
class EType_TypeVar : public EType
{
    int index;
    // std::string name;

public:
    void Accept(ETypeVisitor& visitor) override { visitor.Visit(this); }
};

class EType_Void : public EType
{
public:
    void Accept(ETypeVisitor& visitor) override { visitor.Visit(this); }
};

class ETupleVar
{
    EType* declType;
    std::string name;
};

class EType_Tuple : public EType
{
    std::vector<ETupleVar> vars;

public:
    void Accept(ETypeVisitor& visitor) override { visitor.Visit(this); }
};

class EType_Func : public EType
{
    bool bLocal;
    EFuncReturn funcRet;
    std::vector<EFuncParameter> parameters;

public:
    void Accept(ETypeVisitor& visitor) override { visitor.Visit(this); }
};

class EType_Ptr : public EType
{
    EType* innerType;

public:
    void Accept(ETypeVisitor& visitor) override { visitor.Visit(this); }
};

class EType_Box : public EType
{
    EType* innerType;
public:
    void Accept(ETypeVisitor& visitor) override { visitor.Visit(this); }
};

class EType_Instance : public EType
{
    EDeclId* declId;
    ETypeArguments* typeArgs;

public:
    void Accept(ETypeVisitor& visitor) override { visitor.Visit(this); }
};


}

