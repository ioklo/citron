#pragma once
#include "IR0Config.h"

#include <variant>
#include <vector>
#include "RType.h"
#include "RNames.h"
#include "NArgument.h"

namespace Citron {

class NStmt_Command;
class NStmt_LocalVarDecl;
class NStmt_If;
class NStmt_IfNullableRefTest;
class NStmt_IfNullableValueTest;
class NStmt_For;
class NStmt_Continue;
class NStmt_Break;
class NStmt_Return;
class NStmt_Block;
class NStmt_Blank;
class NStmt_Exp;
class NStmt_Task;
class NStmt_Await;
class NStmt_Async;
class NStmt_Foreach;
class NStmt_ForeachCast;
class NStmt_Yield;
class NStmt_CallClassCtor;
class NStmt_CallStructCtor;
class NStmt_NullDirective;
class NStmt_NotNullDirective;
class NStmt_StaticNullDirective;
class NStmt_StaticNotNullDirective;
class NStmt_StaticUnknownNullDirective;

class NLambdaDecl;
class NStructCtorDecl;
class RClassCtorDecl;

class NExp_String;
using NExpPtr = std::shared_ptr<class NExp>;
using NStmtPtr = std::shared_ptr<class NStmt>;
using NLocPtr = std::shared_ptr<class NLoc>;

class NStmtVisitor
{
public:
    virtual ~NStmtVisitor() { }
    virtual void Visit(NStmt_Command& stmt) = 0;
    virtual void Visit(NStmt_LocalVarDecl& stmt) = 0;
    virtual void Visit(NStmt_If& stmt) = 0;
    virtual void Visit(NStmt_IfNullableRefTest& stmt) = 0;
    virtual void Visit(NStmt_IfNullableValueTest& stmt) = 0;
    virtual void Visit(NStmt_For& stmt) = 0;
    virtual void Visit(NStmt_Continue& stmt) = 0;
    virtual void Visit(NStmt_Break& stmt) = 0;
    virtual void Visit(NStmt_Return& stmt) = 0;
    virtual void Visit(NStmt_Block& stmt) = 0;
    virtual void Visit(NStmt_Blank& stmt) = 0;
    virtual void Visit(NStmt_Exp& stmt) = 0;
    virtual void Visit(NStmt_Task& stmt) = 0;
    virtual void Visit(NStmt_Await& stmt) = 0;
    virtual void Visit(NStmt_Async& stmt) = 0;
    virtual void Visit(NStmt_Foreach& stmt) = 0;
    virtual void Visit(NStmt_ForeachCast& stmt) = 0;
    virtual void Visit(NStmt_Yield& stmt) = 0;
    virtual void Visit(NStmt_CallClassCtor& stmt) = 0;
    virtual void Visit(NStmt_CallStructCtor& stmt) = 0;
    virtual void Visit(NStmt_NullDirective& stmt) = 0;
    virtual void Visit(NStmt_NotNullDirective& stmt) = 0;
    virtual void Visit(NStmt_StaticNullDirective& stmt) = 0;
    virtual void Visit(NStmt_StaticNotNullDirective& stmt) = 0;
    virtual void Visit(NStmt_StaticUnknownNullDirective& stmt) = 0;
};

class NStmt
{
public:
    virtual ~NStmt() { }
    virtual void Accept(NStmtVisitor& visitor) = 0;
};

class NStmt_Command : public NStmt
{
public:
    std::vector<std::shared_ptr<NExp_String>> commands;
public:
    IR0_API NStmt_Command(std::vector<std::shared_ptr<NExp_String>>&& commands);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this);  }
};

// 로컬 변수는 
class NStmt_LocalVarDecl : public NStmt
{
public:
    RTypePtr type;
    std::string name;
    NExpPtr initExp;
public:
    IR0_API NStmt_LocalVarDecl(const RTypePtr& type, const std::string& name, NExpPtr&& initExp);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_If : public NStmt
{
public:
    NExpPtr cond;
    std::vector<NStmtPtr> body;
    std::vector<NStmtPtr> elseBody;
public:
    IR0_API NStmt_If(NExpPtr&& cond, std::vector<NStmtPtr>&& body, std::vector<NStmtPtr>&& elseBody);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_IfNullableRefTest : public NStmt
{
public:
    RTypePtr refType;
    RName varName;
    NExpPtr asExp;
    std::vector<NStmtPtr> body;
    std::vector<NStmtPtr> elseBody;
public:
    IR0_API NStmt_IfNullableRefTest(RTypePtr&& refType, RName&& varName, NExpPtr&& asExp, std::vector<NStmtPtr>&& body, std::vector<NStmtPtr>&& elseBody);

    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_IfNullableValueTest : public NStmt
{
public:
    RTypePtr type;
    RName varName;
    NExpPtr asExp;
    std::vector<NStmtPtr> body;
    std::vector<NStmtPtr> elseBody;
public:
    IR0_API NStmt_IfNullableValueTest(RTypePtr&& type, RName&& varName, NExpPtr&& asExp, std::vector<NStmtPtr>&& body, std::vector<NStmtPtr>&& elseBody);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_For : public NStmt
{
public:
    std::vector<NStmtPtr> initStmts;
    NExpPtr condExp;
    NExpPtr continueExp;
    std::vector<NStmtPtr> body;
public:
    IR0_API NStmt_For(std::vector<NStmtPtr>&& initStmts, NExpPtr&& condExp, NExpPtr&& continueExp, std::vector<NStmtPtr>&& body);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Continue : public NStmt
{
public:
    IR0_API NStmt_Continue();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Break : public NStmt
{
public:
    IR0_API NStmt_Break();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Return : public NStmt
{
public:
    NExpPtr exp;
public:
    IR0_API NStmt_Return(NExpPtr&& exp);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Block : public NStmt
{
public:
    std::vector<NStmtPtr> stmts;
public:
    IR0_API NStmt_Block(std::vector<NStmtPtr>&& stmts);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Blank : public NStmt
{
public:
    IR0_API NStmt_Blank();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Exp : public NStmt
{
public:
    NExpPtr exp;
public:
    IR0_API NStmt_Exp(NExpPtr&& exp);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Task : public NStmt
{
public:
    std::shared_ptr<NLambdaDecl> lambdaDecl;
    std::vector<NArgument> captureArgs;
public:
    IR0_API NStmt_Task(std::shared_ptr<NLambdaDecl>&& lambdaDecl, std::vector<NArgument>&& captureArgs);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Await : public NStmt
{
public:
    std::vector<NStmtPtr> body;
public:
    IR0_API NStmt_Await(std::vector<NStmtPtr>&& body);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Async : public NStmt
{
public:
    std::shared_ptr<NLambdaDecl> lambdaDecl;
    std::vector<NArgument> captureArgs;
public:
    IR0_API NStmt_Async(std::shared_ptr<NLambdaDecl>&& lambdaDecl, std::vector<NArgument>&& captureArgs);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Foreach : public NStmt
{
public:
    NExpPtr enumeratorExp;
    RTypePtr itemType;
    RName varName;
    NExpPtr nextExp;
    std::vector<NStmtPtr> body;
public:
    IR0_API NStmt_Foreach(NExpPtr&& enumeratorExp, RTypePtr&& itemType, const RName& varName, NExpPtr&& nextExp, std::vector<NStmtPtr>&& body);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_ForeachCast : public NStmt
{
public:    
    NExpPtr enumeratorExp;
    RTypePtr itemType;
    RName varName;
    RTypePtr rawItemType;
    NExpPtr nextExp;
    NExpPtr castExp;
    std::vector<NStmtPtr> body;
public:
    IR0_API NStmt_ForeachCast(NExpPtr&& enumeratorExp, RTypePtr&& itemType, const RName& varName, RTypePtr&& rawItemType, NExpPtr&& nextExp, NExpPtr&& castExp, std::vector<NStmtPtr>&& body);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Yield : public NStmt
{
public:
    NExpPtr value;
public:
    IR0_API NStmt_Yield(NExpPtr&& value);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

// Ctor 내에서 상위 Ctor 호출시 사용
class NStmt_CallClassCtor : public NStmt
{
public:
    std::shared_ptr<RClassCtorDecl> ctor;
    std::vector<NArgument> args;
public:
    IR0_API NStmt_CallClassCtor();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_CallStructCtor : public NStmt
{
public:
    std::shared_ptr<NStructCtorDecl> ctor;
    RTypeArgumentsPtr typeArgs;
    std::vector<NArgument> args;
public:
    IR0_API NStmt_CallStructCtor();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_NullDirective : public NStmt
{
public:
    NLocPtr loc;
public:
    IR0_API NStmt_NullDirective(NLocPtr&& loc);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_NotNullDirective : public NStmt
{
public:
    NLocPtr loc;
public:
    IR0_API NStmt_NotNullDirective(NLocPtr&& loc);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_StaticNullDirective : public NStmt
{
public:
    NLocPtr loc;
public:
    IR0_API NStmt_StaticNullDirective();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_StaticNotNullDirective : public NStmt
{
public:
    NLocPtr loc;
public:
    IR0_API NStmt_StaticNotNullDirective();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_StaticUnknownNullDirective : public NStmt
{
public:
    NLocPtr loc;
public:
    IR0_API NStmt_StaticUnknownNullDirective();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

}