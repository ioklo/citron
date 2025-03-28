export module Citron.MSymbol:MBodyDeclOuter;

import <variant>;
import <memory>;

import :ForwardDecls;

namespace Citron
{

export class MBodyDeclOuterVisitor
{
public:
    virtual ~MBodyDeclOuterVisitor() = default;
    virtual void Visit(MGlobalFuncDecl& outer) = 0;
    virtual void Visit(MClassCtorDecl& outer) = 0;
    virtual void Visit(MClassFuncDecl& outer) = 0;
    virtual void Visit(MStructCtorDecl& outer) = 0;
    virtual void Visit(MStructFuncDecl& outer) = 0;
};

// 이것은 weak_ptr로 선언하도록 한다
export class MBodyDeclOuter
{
public:
    virtual ~MBodyDeclOuter() = default;
    virtual void Accept(MBodyDeclOuterVisitor& visitor) = 0;
};

export using MBodyDeclOuterWPtr = std::weak_ptr<MBodyDeclOuter>;

}