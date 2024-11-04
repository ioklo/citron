#pragma once

#include <memory>
#include "RTypeDeclOuter.h"

namespace Citron
{

class NDecl;
class NModuleDecl;
class NNamespaceDecl;
class NClassDecl;
class NStructDecl;

class NTypeDeclOuterVisitor
{
public:
    virtual ~NTypeDeclOuterVisitor() { }
    virtual void Visit(NModuleDecl& outer) = 0;
    virtual void Visit(NNamespaceDecl& outer) = 0;
    virtual void Visit(NClassDecl& outer) = 0;
    virtual void Visit(NStructDecl& outer) = 0;
};

// 보통 타입의 Outer
class NTypeDeclOuter
{
public:
    virtual ~NTypeDeclOuter() { }
    virtual NDecl* GetNDecl() = 0;
    virtual void Accept(NTypeDeclOuterVisitor& visitor) = 0;
};

// 역링크이기 때문에 weak로 설정한다
using NTypeDeclOuterWPtr = std::weak_ptr<NTypeDeclOuter>;

}