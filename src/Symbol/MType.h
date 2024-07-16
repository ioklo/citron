#pragma once

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
class MSymbolType;

class MTypeVisitor
{
public:
    virtual void Visit(MNullableType& type) = 0;
    virtual void Visit(MTypeVarType& type) = 0;
    virtual void Visit(MVoidType& type) = 0;
    virtual void Visit(MTupleType& type) = 0;
    virtual void Visit(MFuncType& type) = 0;
    virtual void Visit(MLocalPtrType& type) = 0;
    virtual void Visit(MBoxPtrType& type) = 0;
    virtual void Visit(MSymbolType& type) = 0;
};

class MType
{
public:
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
class MTypeVarType : public MType
{
    int index;
    std::string name;

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

using MFuncTypePtr = std::shared_ptr<MFuncType>;

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

class MSymbolType : public MType
{
public:
    void Accept(MTypeVisitor& visitor) override { visitor.Visit(*this); }
};


}

