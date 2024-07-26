#pragma once

#include <memory>

namespace Citron
{

class RModuleDecl;
class RNamespaceDecl;
class RClassDecl;
class RStructDecl;

class RTypeDeclOuterVisitor
{
public:
    virtual ~RTypeDeclOuterVisitor() { }
    virtual void Visit(RModuleDecl& outer) = 0;
    virtual void Visit(RNamespaceDecl& outer) = 0;
    virtual void Visit(RClassDecl& outer) = 0;
    virtual void Visit(RStructDecl& outer) = 0;
};

// 보통 타입의 Outer
class RTypeDeclOuter
{
public:
    virtual ~RTypeDeclOuter() { }
    virtual void Accept(RTypeDeclOuterVisitor& visitor) = 0;
};

// 역링크이기 때문에 weak로 설정한다
using RTypeDeclOuterPtr = std::weak_ptr<RTypeDeclOuter>;

}