export module Citron.NDecls:NTypeDeclOuter;

import <memory>;
import Citron.RDecls;

namespace Citron
{

export class NNamespaceDecl;
export class NClassDecl;
export class NStructDecl;
export class NDecl;

export class NTypeDeclOuterVisitor
{
public:
    virtual ~NTypeDeclOuterVisitor() {}
    virtual void Visit(NNamespaceDecl& outer) = 0;
    virtual void Visit(NClassDecl& outer) = 0;
    virtual void Visit(NStructDecl& outer) = 0;
};

// 보통 타입의 Outer
export class NTypeDeclOuter
{
public:
    virtual ~NTypeDeclOuter() {}
    virtual NDecl* GetNDecl() = 0;
    virtual void Accept(NTypeDeclOuterVisitor& visitor) = 0;
};

// 역링크이기 때문에 weak로 설정한다
export using NTypeDeclOuterWPtr = std::weak_ptr<NTypeDeclOuter>;

}