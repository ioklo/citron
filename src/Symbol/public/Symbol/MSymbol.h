#pragma once

namespace Citron
{

class MGlobalFunc;
class MClass;
class MClassConstructor;
class MClassMemberFunc;
class MClassMemberVar;
class MStruct;
class MStructConstructor;
class MStructMemberFunc;
class MStructMemberVar;
class MEnum;
class MEnumElem;
class MEnumElemMemberVar;
class MLambda;
class MLambdaMemberVar;
class MInterface;

class MSymbolVisitor
{
public:
    virtual ~MSymbolVisitor() { }
    virtual void Visit(MGlobalFunc& symbol) = 0;
    virtual void Visit(MClass& symbol) = 0;
    virtual void Visit(MClassConstructor& symbol) = 0;
    virtual void Visit(MClassMemberFunc& symbol) = 0;
    virtual void Visit(MClassMemberVar& symbol) = 0;
    virtual void Visit(MStruct& symbol) = 0;
    virtual void Visit(MStructConstructor& symbol) = 0;
    virtual void Visit(MStructMemberFunc& symbol) = 0;
    virtual void Visit(MStructMemberVar& symbol) = 0;
    virtual void Visit(MEnum& symbol) = 0;
    virtual void Visit(MEnumElem& symbol) = 0;
    virtual void Visit(MEnumElemMemberVar& symbol) = 0;
    virtual void Visit(MLambda& symbol) = 0;
    virtual void Visit(MLambdaMemberVar& symbol) = 0;
    // virtual void Visit(MInterface& symbol) = 0;
};

class MSymbol
{
public:
    virtual ~MSymbol() { }
    virtual void Accept(MSymbolVisitor& visitor) = 0;
};

}