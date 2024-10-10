#pragma once

#include <memory>
#include <IR0/RTypeArguments.h>
#include <IR0/RType.h>
#include <IR0/RLoc.h>

namespace Citron {

class RNamespaceDecl;
class RType_TypeVar;
class RClassDecl;
class RStructDecl;
class REnumDecl;

namespace SyntaxIR0Translator {

// Intermediate Ref Exp

class IrExp_Namespace;
class IrExp_TypeVar;
class IrExp_Class;
class IrExp_Struct;
class IrExp_Enum;
class IrExp_ThisVar;
class IrExp_StaticRef;
class IrExp_BoxRef;
class IrExp_BoxRef_ClassMember;
class IrExp_BoxRef_StructIndirectMember;
class IrExp_BoxRef_StructMember;
class IrExp_LocalRef;
class IrExp_DerefedBoxValue;
class IrExp_LocalValue;

class IrExpVisitor;

class IrExp
{
public:
    virtual void Accept(IrExpVisitor& visitor) = 0;
};

using IrExpPtr = std::shared_ptr<IrExp>;

class IrExpVisitor
{
public:
    virtual void Visit(IrExp_Namespace& irExp) = 0;
    virtual void Visit(IrExp_TypeVar& irExp) = 0;
    virtual void Visit(IrExp_Class& irExp) = 0;
    virtual void Visit(IrExp_Struct& irExp) = 0;
    virtual void Visit(IrExp_Enum& irExp) = 0;
    virtual void Visit(IrExp_ThisVar& irExp) = 0;
    virtual void Visit(IrExp_StaticRef& irExp) = 0;
    virtual void Visit(IrExp_BoxRef& irExp) = 0;
    virtual void Visit(IrExp_LocalRef& irExp) = 0;
    virtual void Visit(IrExp_DerefedBoxValue& irExp) = 0;
    virtual void Visit(IrExp_LocalValue& irExp) = 0;
};

class IrExp_Namespace : public IrExp
{
public:
    std::shared_ptr<RNamespaceDecl> decl;

public:
    IrExp_Namespace(const std::shared_ptr<RNamespaceDecl>& decl);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

class IrExp_TypeVar : public IrExp
{
public:
    std::shared_ptr<RType_TypeVar> type;

public:
    IrExp_TypeVar(const std::shared_ptr<RType_TypeVar>& type);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

class IrExp_Class : public IrExp
{
public:
    std::shared_ptr<RClassDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IrExp_Class(const std::shared_ptr<RClassDecl>& decl, RTypeArgumentsPtr&& typeArgs);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

class IrExp_Struct : public IrExp
{
public:
    std::shared_ptr<RStructDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IrExp_Struct(const std::shared_ptr<RStructDecl>& decl, RTypeArgumentsPtr&& typeArgs);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

class IrExp_Enum : public IrExp
{
public:
    std::shared_ptr<REnumDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IrExp_Enum(const std::shared_ptr<REnumDecl>& decl, RTypeArgumentsPtr&& typeArgs);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 자체로는 invalid하지만 memberExp랑 결합되면 의미가 생기기때문에 정보를 갖고 있는다
class IrExp_ThisVar : public IrExp
{
public:
    RTypePtr type;

public:
    IrExp_ThisVar(const RTypePtr& type);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

// exp로 사용할 수 있는
//class LocalVar(IType Type, Name Name) : public IrExp
//{
//    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalVar(this);
//}

//class LambdaMemberVar(LambdaMemberVarSymbol Symbol) : public IrExp
//{
//    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambdaMemberVar(this);
//}

//class ClassMemberVar(ClassMemberVarSymbol Symbol, bool HasExplicitInstance, ResolvedExp? ExplicitInstance) : public IrExp
//{
//    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberVar(this);
//}

//class StructMemberVar(StructMemberVarSymbol Symbol, bool HasExplicitInstance, ResolvedExp? ExplicitInstance) : public IrExp
//{
//    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberVar(this);
//}

//class EnumElemMemberVar(EnumElemMemberVarSymbol Symbol, ResolvedExp Instance) : public IrExp
//{
//    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElemMemberVar(this);
//}

//class ListIndexer(ResolvedExp Instance, R.Exp Index, IType ItemType) : public IrExp
//{
//    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitListIndexer(this);
//}

//class LocalDeref(ResolvedExp Target) : public IrExp
//{
//    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLocalDeref(this);
//}

//class BoxDeref(ResolvedExp Target) : public IrExp
//{
//    public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBoxDeref(this);
//}

// &C.x, holder없이 주소가 살아있는
class IrExp_StaticRef : public IrExp
{
public:
    RLocPtr loc;

public:
    IrExp_StaticRef(const RLocPtr& loc);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

class IrBoxRefExpVisitor
{
public:
    virtual void Visit(IrExp_BoxRef_ClassMember& irBoxRefExp) = 0;
    virtual void Visit(IrExp_BoxRef_StructIndirectMember& irBoxRefExp) = 0;
    virtual void Visit(IrExp_BoxRef_StructMember& irBoxRefExp) = 0;
};

class IrExp_BoxRef : public IrExp
{
public:
    void Accept(IrExpVisitor& visitor) final { visitor.Visit(*this); }

    virtual void Accept(IrBoxRefExpVisitor& visitor) = 0;
    virtual RTypePtr GetTargetType(RTypeFactory& factory) = 0;
    virtual RLocPtr MakeLoc() = 0;
};

// 홀더가 C로 시작하는 경우
class IrExp_BoxRef_ClassMember : public IrExp_BoxRef
{
public:
    RLocPtr loc;
    std::shared_ptr<RClassMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IrExp_BoxRef_ClassMember(const RLocPtr& loc, const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(IrBoxRefExpVisitor& visitor) override { visitor.Visit(*this); }

    RTypePtr GetTargetType(RTypeFactory& factory) override;
    RLocPtr MakeLoc() override;
};

// 홀더가 box* T로 시작하는 경우
// (*pS).x => Exp는 pS를 말한다
class IrExp_BoxRef_StructIndirectMember : public IrExp_BoxRef
{
public:
    RLocPtr loc;
    std::shared_ptr<RStructMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IrExp_BoxRef_StructIndirectMember(const RLocPtr& loc, const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(IrBoxRefExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetTargetType(RTypeFactory& factory) override;
    RLocPtr MakeLoc() override;
};

class IrExp_BoxRef_StructMember : public IrExp_BoxRef
{
public:
    std::shared_ptr<IrExp_BoxRef> parent;
    std::shared_ptr<RStructMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IrExp_BoxRef_StructMember(const std::shared_ptr<IrExp_BoxRef>& parent, const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(IrBoxRefExpVisitor & visitor) override { visitor.Visit(*this); }
    RTypePtr GetTargetType(RTypeFactory& factory) override;
    RLocPtr MakeLoc() override;
};

class IrExp_LocalRef : public IrExp
{
public:
    RLocPtr loc;

public:
    IrExp_LocalRef(const RLocPtr& loc);
    void Accept(IrExpVisitor & visitor) override { visitor.Visit(*this); }
};

// Value로 나오는 경우
class IrExp_LocalValue : public IrExp
{
public:
    RExpPtr exp;

public:
    IrExp_LocalValue(RExpPtr&& exp);
    void Accept(IrExpVisitor & visitor) override { visitor.Visit(*this); }
};

// handle
// box S* pS;
// box var* x = &(*pS).a; // x is box ptr (pS, ..)

// *pS <- BoxValue, box S
class IrExp_DerefedBoxValue : public IrExp
{   
public:
    RLocPtr innerLoc;

public:
    IrExp_DerefedBoxValue(RLocPtr&& innerLoc);
    void Accept(IrExpVisitor & visitor) override { visitor.Visit(*this); }
};

} // SyntaxIR0Translator

} // Citron