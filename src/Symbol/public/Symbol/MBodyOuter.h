#pragma once
#include <variant>
#include <memory>

namespace Citron
{
class MGlobalFunc;
class MClassConstructor;
class MClassMemberFunc;
class MStructConstructor;
class MStructMemberFunc;
class MLambda;

class MBodyOuterVisitor
{
public:
    virtual ~MBodyOuterVisitor() { }
    virtual void Visit(MGlobalFunc& outer) = 0;
    virtual void Visit(MClassConstructor& outer) = 0;
    virtual void Visit(MClassMemberFunc& outer) = 0;
    virtual void Visit(MStructConstructor& outer) = 0;
    virtual void Visit(MStructMemberFunc& outer) = 0;
    virtual void Visit(MLambda& outer) = 0;
};

class MBodyOuter
{
public:
    virtual ~MBodyOuter() { }
    virtual void Accept(MBodyOuterVisitor& visitor) = 0;
};

}