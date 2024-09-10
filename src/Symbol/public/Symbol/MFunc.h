#pragma once

namespace Citron
{

class MGlobalFunc;        // top-level decl space
class MClassConstructor;  // construct decl space
class MClassMemberFunc;   // construct decl space
class MStructConstructor; // struct decl space
class MStructMemberFunc;  // struct decl space

class MFuncVisitor
{
public:
    virtual ~MFuncVisitor() { }
    virtual void Visit(MGlobalFunc& func) = 0;
    virtual void Visit(MClassConstructor& func) = 0;
    virtual void Visit(MClassMemberFunc& func) = 0;
    virtual void Visit(MStructConstructor& func) = 0;
    virtual void Visit(MStructMemberFunc& func) = 0;
};

class MFunc
{
public:
    virtual ~MFunc() { }
    virtual void Accept(MFuncVisitor& visitor) = 0;
};

}