export module Citron.MSymbol:MType;

import <vector>;
import <memory>;
import <string>;

import :MFuncReturn;
import :MFuncParameter;

namespace Citron
{

// 같은 unit내에서의 forward declaration
class MType_Nullable;
class MType_TypeVar;  // 이것은 Symbol인가?
class MType_Void;     // builtin type
class MType_Tuple;    // inline type
class MType_Func;     // inline type, circular
class MType_LocalPtr; // inline type
class MType_BoxPtr;   // inline type
class MType_Instance;

export class MTypeVisitor
{
public:
    virtual ~MTypeVisitor() {}
    virtual void Visit(MType_Nullable& type) = 0;
    virtual void Visit(MType_TypeVar& type) = 0;
    virtual void Visit(MType_Void& type) = 0;
    virtual void Visit(MType_Tuple& type) = 0;
    virtual void Visit(MType_Func& type) = 0;
    virtual void Visit(MType_LocalPtr& type) = 0;
    virtual void Visit(MType_BoxPtr& type) = 0;
    virtual void Visit(MType_Instance& type) = 0;
};

export class MType
{
public:
    virtual ~MType() {}
    virtual void Accept(MTypeVisitor& visitor) = 0;
};

export using MTypePtr = std::shared_ptr<MType>;

// recursive types
export class MType_Nullable : public MType
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
export class MType_TypeVar : public MType
{
    int index;
    // std::string name;

public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class MType_Void : public MType
{
public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class MTupleVar
{
    MTypePtr declType;
    std::string name;
};

export class MType_Tuple : public MType
{
    std::vector<MTupleVar> vars;

public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class MType_Func : public MType
{
    bool bLocal;
    MFuncReturn funcRet;
    std::vector<MFuncParameter> parameters;

public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class MType_LocalPtr : public MType
{
    MTypePtr innerType;

public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class MType_BoxPtr : public MType
{
    MTypePtr innerType;
public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class MType_Instance : public MType
{
    MDeclIdPtr declId;
    MTypeArgumentsPtr typeArgs;

public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};


}

