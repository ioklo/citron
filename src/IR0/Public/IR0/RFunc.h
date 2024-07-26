#pragma once

namespace Citron
{

class RGlobalFunc;        // top-level decl space
class RClassConstructor;  // construct decl space
class RClassMemberFunc;   // construct decl space
class RStructConstructor; // struct decl space
class RStructMemberFunc;  // struct decl space
class RLambda;            // body space

class RFuncVisitor
{
public:
    virtual ~RFuncVisitor() { }
    virtual void Visit(RGlobalFunc& func) = 0;
    virtual void Visit(RClassConstructor& func) = 0;
    virtual void Visit(RClassMemberFunc& func) = 0;
    virtual void Visit(RStructConstructor& func) = 0;
    virtual void Visit(RStructMemberFunc& func) = 0;
    virtual void Visit(RLambda& func) = 0;
};

class RFunc
{
public:
    virtual ~RFunc() { }
    virtual void Accept(RFuncVisitor& visitor) = 0;
};

using RFuncPtr = std::shared_ptr<RFunc>;

}