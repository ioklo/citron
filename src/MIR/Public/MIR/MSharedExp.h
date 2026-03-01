#pragma once
#include "MIRConfig.h"

#include <memory>

namespace Citron {

class RType;
class RTypeArguments;
class RClassVarDecl;
class RStructVarDecl;

class MLoc;

using RFactoryPtr = std::shared_ptr<class RFactory>;

class MSharedExpVisitor;

class MSharedExp_Static;
class MSharedExp_ClassVar;
class MSharedExp_SharedStructVar;
class MSharedExp_StructVar;

// &연산으로 shared를 만들어내는 exp
class MSharedExp
{
public:
    virtual ~MSharedExp() {}
    virtual RType* GetType() = 0;
    virtual void Accept(MSharedExpVisitor& visitor) = 0;
};

class MSharedExpVisitor
{
public:
    virtual ~MSharedExpVisitor() {}
    virtual void Visit(MSharedExp_Static* sharedExp) = 0;
    virtual void Visit(MSharedExp_ClassVar* sharedExp) = 0;
    virtual void Visit(MSharedExp_SharedStructVar* sharedExp) = 0;
    virtual void Visit(MSharedExp_StructVar* sharedExp) = 0;
};

// &C.x
class MSharedExp_Static : public MSharedExp
{
    RFactoryPtr rFactory;
public:
    MLoc* loc;

public:
    MIR_API MSharedExp_Static(MLoc* loc, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MSharedExpVisitor& visitor) override { visitor.Visit(this); }
};

// &c.x => MSharedExp_ClassVar(MLoc_LocalVar("c"), C::x)
class MSharedExp_ClassVar : public MSharedExp
{
    RFactoryPtr rFactory;
public:
    MLoc* base;
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MSharedExp_ClassVar(MLoc* base, RClassVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MSharedExpVisitor& visitor) override { visitor.Visit(this); }
};

// box S* pS;
// &ps->x => MSharedExp_SharedStructVar(MLoc_LocalVar("pS"), S::x)
// MSharedExp_SharedStructVar
class MSharedExp_SharedStructVar : public MSharedExp
{
    RFactoryPtr rFactory;
public:
    MLoc* base;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MSharedExp_SharedStructVar(MLoc* base, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MSharedExpVisitor& visitor) override { visitor.Visit(this); }
};

// C c;
// shared A a = &c.s.a; => MSharedExp_StructVar(MSharedExp_ClassVar(MLoc_LocalVar("c"), C::s), A::a)
class MSharedExp_StructVar : public MSharedExp
{
public:
    MSharedExp* base;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;
    RFactoryPtr rFactory;

public:
    MIR_API MSharedExp_StructVar(MSharedExp* base, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory);

    MIR_API RType* GetType() override;
    void Accept(MSharedExpVisitor& visitor) override { visitor.Visit(this); }
};

} // namespace Citron