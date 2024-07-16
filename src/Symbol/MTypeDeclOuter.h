#pragma once

namespace Citron
{

class MModuleDecl;
class MNamespaceDecl;
class MClassDecl;
class MStructDecl;

class MTypeDeclOuterVisitor
{
public:
    virtual void Visit(MModuleDecl& outer) = 0;
    virtual void Visit(MNamespaceDecl& outer) = 0;
    virtual void Visit(MClassDecl& outer) = 0;
    virtual void Visit(MStructDecl& outer) = 0;
};

// 보통 타입의 Outer
class MTypeDeclOuter
{
public:
    virtual void Accept(MTypeDeclOuterVisitor& visitor) = 0;
};

// 역링크이기 때문에 weak로 설정한다
using MTypeDeclOuterPtr = std::weak_ptr<MTypeDeclOuter>;

}