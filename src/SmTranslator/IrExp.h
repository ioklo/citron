#pragma once

#include <optional>
#include <memory>
#include "MIR/MRead.h"

namespace Citron {

class RNamespace;
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
struct MLoc;
using MFactoryPtr = std::shared_ptr<class MFactory>;
struct TranslationContexts;

// Intermediate SharedRef Exp, 일반 ptr변환은 여기를 거치지 않도록 한다
// Syntax가 &exp 꼴일 경우 IrExp를 거쳐서 ReExp(ResolvedExp)로 변환한다

struct IrExpVisitor;

struct IrExp
{
public:
    virtual ~IrExp() {}
    virtual void Accept(IrExpVisitor& visitor) = 0;
};

struct IrExp_Namespaces : IrExp
{
    RNamespaceGroup namespaces;

    IrExp_Namespaces(RNamespaceGroup&& namespaces);
    void Accept(IrExpVisitor& visitor) override;
};

struct IrExp_Class : IrExp
{
    RClassDecl* decl;
    RTypeArguments* typeArgs;

    IrExp_Class(RClassDecl* decl, RTypeArguments* typeArgs);
    void Accept(IrExpVisitor& visitor) override;
};

struct IrExp_Struct : IrExp
{
    RStructDecl* decl;
    RTypeArguments* typeArgs;

    IrExp_Struct(RStructDecl* decl, RTypeArguments* typeArgs);
    void Accept(IrExpVisitor& visitor) override;
};

// &C.x
struct IrExp_Static : IrExp
{
    MLoc* loc; // MLoc_ClassVar 혹은 MLoc_StructVar였을 것

    IrExp_Static(MLoc* loc)
        : loc{loc}
    { }
    void Accept(IrExpVisitor& visitor) override;
};

// &c.x => IrExp_ClassVar(MLoc_LocalVar("c"), C::x)
struct IrExp_ClassVar : IrExp
{
    MLoc* base;
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

    IrExp_ClassVar(MLoc* base, RClassVarDecl* decl, RTypeArguments* typeArgs)
        : base{base}, decl{decl}, typeArgs{typeArgs}
    { }
    void Accept(IrExpVisitor& visitor) override;
};

// shared S pS;
// &ps->x => IrExp_SharedStructVar(MLoc_LocalVar("pS"), S::x)
// IrExp_SharedStructVar
struct IrExp_SharedStructVar : IrExp
{
    MLoc* base;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

    IrExp_SharedStructVar(MLoc* base, RStructVarDecl* decl, RTypeArguments* typeArgs)
        : base{base}, decl{decl}, typeArgs{typeArgs}
    { }
    void Accept(IrExpVisitor& visitor) override;
};

// C c;
// shared A a = &c.s.a; => IrExp_StructVar(IrExp_ClassVar(MLoc_LocalVar("c"), C::s), A::a)
struct IrExp_StructVar : IrExp
{
    IrExp* base;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

    IrExp_StructVar(IrExp* base, RStructVarDecl* decl, RTypeArguments* typeArgs)
        : base{base}, decl{decl}, typeArgs{typeArgs}
    { }
    void Accept(IrExpVisitor& visitor) override;
};

// (*pS).id 를 처리하기 위해서
// *pS 모양을 따로 들고 있는다. IrExp_Loc{MLoc_SharedDeref}는 만들어지면 안된다
struct IrExp_SharedDeref : IrExp
{
    MRead_Loc srcShared;

    IrExp_SharedDeref(MRead_Loc&& srcShared)
        : srcShared{std::move(srcShared)}
    { }
    void Accept(IrExpVisitor& visitor) override;
};

struct IrExp_Loc : IrExp
{
    MLoc* loc;

    IrExp_Loc(MLoc* loc);
    void Accept(IrExpVisitor& visitor) override;
};

} // namespace Citron

#include "IrExpVisitor.g.h"