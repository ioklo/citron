#pragma once
#include <variant>
#include <memory>

namespace Citron
{
class RGlobalFunc;
class RClassConstructor;
class RClassMemberFunc;
class RStructConstructor;
class RStructMemberFunc;
class RLambda;

class RBodyOuterVisitor
{
public:
    virtual ~RBodyOuterVisitor() { }
    virtual void Visit(RGlobalFunc& outer) = 0;
    virtual void Visit(RClassConstructor& outer) = 0;
    virtual void Visit(RClassMemberFunc& outer) = 0;
    virtual void Visit(RStructConstructor& outer) = 0;
    virtual void Visit(RStructMemberFunc& outer) = 0;
    virtual void Visit(RLambda& outer) = 0;
};

class RBodyOuter
{
public:
    virtual ~RBodyOuter() { }
    virtual void Accept(RBodyOuterVisitor& visitor) = 0;
};

using RBodyOuterPtr = std::weak_ptr<RBodyOuter>;

}