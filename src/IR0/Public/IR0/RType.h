#pragma once

#include <memory>
#include <vector>

#include "RFuncReturn.h"
#include "RFuncParameter.h"

namespace Citron 
{

class RNullableType;
class RTypeVarType;  // 이것은 Symbol인가?
class RVoidType;     // builtin type
class RTupleType;    // inline type
class RFuncType;     // inline type, circular
class RLocalPtrType; // inline type
class RBoxPtrType;   // inline type
class RInstanceType; // indirect
class RDeclId;
using RDeclIdPtr = std::shared_ptr<RDeclId>;

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

class RTypeVisitor
{
public:
    virtual ~RTypeVisitor() { }
    virtual void Visit(RNullableType& type) = 0;
    virtual void Visit(RTypeVarType& type) = 0;
    virtual void Visit(RVoidType& type) = 0;
    virtual void Visit(RTupleType& type) = 0;
    virtual void Visit(RFuncType& type) = 0;
    virtual void Visit(RLocalPtrType& type) = 0;
    virtual void Visit(RBoxPtrType& type) = 0;
    virtual void Visit(RInstanceType& type) = 0;
};

enum class RCustomTypeKind
{
    None,
    Struct,
    Class,
    Enum,
    Interface,
};

class RType
{
public:
    virtual ~RType() { }
    virtual void Accept(RTypeVisitor& visitor) = 0;
    virtual RCustomTypeKind GetCustomTypeKind() { return RCustomTypeKind::None; }
};

using RTypePtr = std::shared_ptr<RType>;

// recursive types
class RNullableType : public RType
{
    RTypePtr innerType;

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

// trivial types
class RTypeVarType : public RType
{
    int index;
    std::string name;

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class RVoidType : public RType
{
public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class MTupleMemberVar
{
    RTypePtr declType;
    std::string name;
};

class RTupleType : public RType
{
    std::vector<MTupleMemberVar> memberVars;

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class RFuncType : public RType
{
    bool bLocal;
    RFuncReturn funcRet;
    std::vector<RFuncParameter> parameters;

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class RLocalPtrType : public RType
{
    RTypePtr innerType;

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class RBoxPtrType : public RType
{
    RTypePtr innerType;
public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class RInstanceType : public RType
{
    RDeclIdPtr declId;
    RTypeArgumentsPtr typeArgs;

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};


}

