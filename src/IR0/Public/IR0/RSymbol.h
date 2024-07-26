#pragma once

#include <memory>

namespace Citron
{

class RGlobalFunc;
class RClass;
class RClassConstructor;
class RClassMemberFunc;
class RClassMemberVar;
class RStruct;
class RStructConstructor;
class RStructMemberFunc;
class RStructMemberVar;
class REnum;
class REnumElem;
class REnumElemMemberVar;
class RLambda;
class RLambdaMemberVar;
class RInterface;

class RSymbolVisitor
{
public:
    virtual ~RSymbolVisitor() { }
    virtual void Visit(RGlobalFunc& symbol) = 0;
    virtual void Visit(RClass& symbol) = 0;
    virtual void Visit(RClassConstructor& symbol) = 0;
    virtual void Visit(RClassMemberFunc& symbol) = 0;
    virtual void Visit(RClassMemberVar& symbol) = 0;
    virtual void Visit(RStruct& symbol) = 0;
    virtual void Visit(RStructConstructor& symbol) = 0;
    virtual void Visit(RStructMemberFunc& symbol) = 0;
    virtual void Visit(RStructMemberVar& symbol) = 0;
    virtual void Visit(REnum& symbol) = 0;
    virtual void Visit(REnumElem& symbol) = 0;
    virtual void Visit(REnumElemMemberVar& symbol) = 0;
    virtual void Visit(RLambda& symbol) = 0;
    virtual void Visit(RLambdaMemberVar& symbol) = 0;
    // virtual void Visit(RInterface& symbol) = 0;
};

class RSymbol
{
public:
    virtual ~RSymbol() { }
    virtual void Accept(RSymbolVisitor& visitor) = 0;
};

using RSymbolPtr = std::shared_ptr<RSymbol>;

}