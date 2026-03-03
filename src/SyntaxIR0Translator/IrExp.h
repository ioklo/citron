#pragma once

#include <optional>
#include <memory>

namespace Citron {

class RNamespaceDecl;
class RType_TypeVar;
class RClassDecl;
class RTypeArguments;
class RStructDecl;
class REnumDecl;
class RType;
class RFactory;
class RClassVarDecl;
class RStructVarDecl;
struct MExp;
class MLoc;
using MFactoryPtr = std::shared_ptr<class MFactory>;
struct TranslationContexts;

// Intermediate SharedRef Exp, 일반 ptr변환은 여기를 거치지 않도록 한다
// Syntax가 &exp 꼴일 경우 IrExp를 거쳐서 ReExp(ResolvedExp)로 변환한다

struct IrExpVisitor;

class IrExp
{
public:
    virtual ~IrExp() {}
    virtual void Accept(IrExpVisitor& visitor) = 0;
};

class IrExp_Namespace : public IrExp
{
public:
    RNamespaceDecl* decl;

public:
    IrExp_Namespace(RNamespaceDecl* decl);
    void Accept(IrExpVisitor& visitor) override;
};

class IrExp_Class : public IrExp
{
public:
    RClassDecl* decl;
    RTypeArguments* typeArgs;

public:
    IrExp_Class(RClassDecl* decl, RTypeArguments* typeArgs);
    void Accept(IrExpVisitor& visitor) override;
};

class IrExp_Struct : public IrExp
{
public:
    RStructDecl* decl;
    RTypeArguments* typeArgs;

public:
    IrExp_Struct(RStructDecl* decl, RTypeArguments* typeArgs);
    void Accept(IrExpVisitor& visitor) override;
};

// &C.x
class IrExp_Static : public IrExp
{
    RFactoryPtr rFactory;
public:
    MLoc* loc;

public:
    IrExp_Static(MLoc* loc, const RFactoryPtr& rFactory)
        : loc{loc}, rFactory{rFactory}
    { }
    void Accept(IrExpVisitor& visitor) override;
};

// &c.x => IrExp_ClassVar(MLoc_LocalVar("c"), C::x)
class IrExp_ClassVar : public IrExp
{
    RFactoryPtr rFactory;
public:
    MLoc* base;
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    IrExp_ClassVar(MLoc* base, RClassVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
        : base{base}, decl{decl}, typeArgs{typeArgs}, rFactory{rFactory}
    { }

    void Accept(IrExpVisitor& visitor) override;
};

// shared S pS;
// &ps->x => IrExp_SharedStructVar(MLoc_LocalVar("pS"), S::x)
// IrExp_SharedStructVar
class IrExp_SharedStructVar : public IrExp
{
    RFactoryPtr rFactory;
public:
    MLoc* base;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    IrExp_SharedStructVar(MLoc* base, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
        : base{base}, decl{decl}, typeArgs{typeArgs}, rFactory{rFactory}
    { }

    void Accept(IrExpVisitor& visitor) override;
};

// C c;
// shared A a = &c.s.a; => IrExp_StructVar(IrExp_ClassVar(MLoc_LocalVar("c"), C::s), A::a)
class IrExp_StructVar : public IrExp
{
public:
    IrExp* base;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

private:
    RFactoryPtr rFactory;

public:
    IrExp_StructVar(IrExp* base, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
        : base{base}, decl{decl}, typeArgs{typeArgs}, rFactory{rFactory}
    { }
    void Accept(IrExpVisitor& visitor) override;
};

// (*pS).id 를 처리하기 위해서
// *x 모양을 따로 들고 있는다. IrExp_Loc{MLoc_Deref}는 만들어지면 안된다
class IrExp_Deref : public IrExp
{
public:
    MLoc* innerLoc;

public:
    IrExp_Deref(MLoc* innerLoc)
        : innerLoc{innerLoc}
    { }
    void Accept(IrExpVisitor& visitor) override;
};

// exp로 나오는 경우
class IrExp_Exp : public IrExp
{
public:
    MExp* exp;

public:
    IrExp_Exp(MExp* exp);
    void Accept(IrExpVisitor& visitor) override;
};

class IrExp_Loc : public IrExp
{
public:
    MLoc* loc;

public:
    IrExp_Loc(MLoc* loc);
    void Accept(IrExpVisitor& visitor) override;
};

} // namespace Citron

#include "IrExpVisitor.g.h"