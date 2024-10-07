#pragma once

#include <memory>
#include <IR0/RTypeArguments.h>
#include <IR0/RType.h>
#include <IR0/RLoc.h>

namespace Citron {

class RNamespaceDecl;
class RTypeVarType;
class RClassDecl;
class RStructDecl;
class REnumDecl;

namespace SyntaxIR0Translator {

// Intermediate Ref Exp

class IrNamespaceExp;
class IrTypeVarExp;
class IrClassExp;
class IrStructExp;
class IrEnumExp;
class IrThisVarExp;
class IrStaticRefExp;
class IrBoxRefExp;
class IrClassMemberBoxRefExp;
class IrStructIndirectMemberBoxRefExp;
class IrStructMemberBoxRefExp;
class IrLocalRefExp;
class IrDerefedBoxValueExp;
class IrLocalValueExp;

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
    virtual void Visit(IrNamespaceExp& irExp) = 0;
    virtual void Visit(IrTypeVarExp& irExp) = 0;
    virtual void Visit(IrClassExp& irExp) = 0;
    virtual void Visit(IrStructExp& irExp) = 0;
    virtual void Visit(IrEnumExp& irExp) = 0;
    virtual void Visit(IrThisVarExp& irExp) = 0;
    virtual void Visit(IrStaticRefExp& irExp) = 0;
    virtual void Visit(IrBoxRefExp& irExp) = 0;
    virtual void Visit(IrLocalRefExp& irExp) = 0;
    virtual void Visit(IrDerefedBoxValueExp& irExp) = 0;
    virtual void Visit(IrLocalValueExp& irExp) = 0;
};

class IrNamespaceExp : public IrExp
{
public:
    std::shared_ptr<RNamespaceDecl> decl;

public:
    IrNamespaceExp(const std::shared_ptr<RNamespaceDecl>& decl);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

class IrTypeVarExp : public IrExp
{
public:
    std::shared_ptr<RTypeVarType> type;

public:
    IrTypeVarExp(const std::shared_ptr<RTypeVarType>& type);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

class IrClassExp : public IrExp
{
public:
    std::shared_ptr<RClassDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IrClassExp(const std::shared_ptr<RClassDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

class IrStructExp : public IrExp
{
public:
    std::shared_ptr<RStructDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IrStructExp(const std::shared_ptr<RStructDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

class IrEnumExp : public IrExp
{
public:
    std::shared_ptr<REnumDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IrEnumExp(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 자체로는 invalid하지만 memberExp랑 결합되면 의미가 생기기때문에 정보를 갖고 있는다
class IrThisVarExp : public IrExp
{
public:
    RTypePtr type;

public:
    IrThisVarExp(const RTypePtr& type);
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
class IrStaticRefExp : public IrExp
{
public:
    RLocPtr loc;

public:
    IrStaticRefExp(const RLocPtr& loc);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(*this); }
};

class IrBoxRefExpVisitor
{
public:
    virtual void Visit(IrClassMemberBoxRefExp& irBoxRefExp) = 0;
    virtual void Visit(IrStructIndirectMemberBoxRefExp& irBoxRefExp) = 0;
    virtual void Visit(IrStructMemberBoxRefExp& irBoxRefExp) = 0;
};

class IrBoxRefExp : public IrExp
{
public:
    void Accept(IrExpVisitor& visitor) final { visitor.Visit(*this); }

    virtual void Accept(IrBoxRefExpVisitor& visitor) = 0;
    virtual RTypePtr GetTargetType(RTypeFactory& factory) = 0;
    virtual RLocPtr MakeLoc() = 0;
};

// 홀더가 C로 시작하는 경우
class IrClassMemberBoxRefExp : public IrBoxRefExp
{
public:
    RLocPtr loc;
    std::shared_ptr<RClassMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IrClassMemberBoxRefExp(const RLocPtr& loc, const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(IrBoxRefExpVisitor& visitor) override { visitor.Visit(*this); }

    RTypePtr GetTargetType(RTypeFactory& factory) override;
    RLocPtr MakeLoc() override;
};

// 홀더가 box* T로 시작하는 경우
// (*pS).x => Exp는 pS를 말한다
class IrStructIndirectMemberBoxRefExp : public IrBoxRefExp
{
public:
    RLocPtr loc;
    std::shared_ptr<RStructMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IrStructIndirectMemberBoxRefExp(const RLocPtr& loc, const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(IrBoxRefExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetTargetType(RTypeFactory& factory) override;
    RLocPtr MakeLoc() override;
};

class IrStructMemberBoxRefExp : public IrBoxRefExp
{
public:
    std::shared_ptr<IrBoxRefExp> parent;
    std::shared_ptr<RStructMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IrStructMemberBoxRefExp(const std::shared_ptr<IrBoxRefExp>& parent, const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(IrBoxRefExpVisitor & visitor) override { visitor.Visit(*this); }
    RTypePtr GetTargetType(RTypeFactory& factory) override;
    RLocPtr MakeLoc() override;
};

class IrLocalRefExp : public IrExp
{
public:
    RLocPtr loc;

public:
    IrLocalRefExp(const RLocPtr& loc);
    void Accept(IrExpVisitor & visitor) override { visitor.Visit(*this); }
};

// Value로 나오는 경우
class IrLocalValueExp : public IrExp
{
public:
    RExpPtr exp;

public:
    IrLocalValueExp(RExpPtr&& exp);
    void Accept(IrExpVisitor & visitor) override { visitor.Visit(*this); }
};

// handle
// box S* pS;
// box var* x = &(*pS).a; // x is box ptr (pS, ..)

// *pS <- BoxValue, box S
class IrDerefedBoxValueExp : public IrExp
{   
public:
    RLocPtr innerLoc;

public:
    IrDerefedBoxValueExp(RLocPtr&& innerLoc);
    void Accept(IrExpVisitor & visitor) override { visitor.Visit(*this); }
};

} // SyntaxIR0Translator

} // Citron