#pragma once
#include "IR0Config.h"

#include <variant>
#include <vector>
#include "RType.h"
#include "RNames.h"
#include "RArgument.h"

namespace Citron {

class RStmt_Command;
class RStmt_LocalVarDecl;
class RStmt_If;
class RStmt_IfNullableRefTest;
class RStmt_IfNullableValueTest;
class RStmt_For;
class RStmt_Continue;
class RStmt_Break;
class RStmt_Return;
class RStmt_Block;
class RStmt_Blank;
class RStmt_Exp;
class RStmt_Task;
class RStmt_Await;
class RStmt_Async;
class RStmt_Foreach;
class RStmt_ForeachCast;
class RStmt_Yield;
class RStmt_CallClassConstructor;
class RStmt_CallStructConstructor;
class RStmt_NullDirective;
class RStmt_NotNullDirective;
class RStmt_StaticNullDirective;
class RStmt_StaticNotNullDirective;
class RStmt_StaticUnknownNullDirective;

class RLambdaDecl;
class RStructConstructorDecl;
class RClassConstructorDecl;

class RExp_String;
using RExpPtr = std::shared_ptr<class RExp>;
using RStmtPtr = std::shared_ptr<class RStmt>;
using RLocPtr = std::shared_ptr<class RLoc>;



class RStmtVisitor
{
public:
    virtual ~RStmtVisitor() { }
    virtual void Visit(RStmt_Command& stmt) = 0;
    virtual void Visit(RStmt_LocalVarDecl& stmt) = 0;
    virtual void Visit(RStmt_If& stmt) = 0;
    virtual void Visit(RStmt_IfNullableRefTest& stmt) = 0;
    virtual void Visit(RStmt_IfNullableValueTest& stmt) = 0;
    virtual void Visit(RStmt_For& stmt) = 0;
    virtual void Visit(RStmt_Continue& stmt) = 0;
    virtual void Visit(RStmt_Break& stmt) = 0;
    virtual void Visit(RStmt_Return& stmt) = 0;
    virtual void Visit(RStmt_Block& stmt) = 0;
    virtual void Visit(RStmt_Blank& stmt) = 0;
    virtual void Visit(RStmt_Exp& stmt) = 0;
    virtual void Visit(RStmt_Task& stmt) = 0;
    virtual void Visit(RStmt_Await& stmt) = 0;
    virtual void Visit(RStmt_Async& stmt) = 0;
    virtual void Visit(RStmt_Foreach& stmt) = 0;
    virtual void Visit(RStmt_ForeachCast& stmt) = 0;
    virtual void Visit(RStmt_Yield& stmt) = 0;
    virtual void Visit(RStmt_CallClassConstructor& stmt) = 0;
    virtual void Visit(RStmt_CallStructConstructor& stmt) = 0;
    virtual void Visit(RStmt_NullDirective& stmt) = 0;
    virtual void Visit(RStmt_NotNullDirective& stmt) = 0;
    virtual void Visit(RStmt_StaticNullDirective& stmt) = 0;
    virtual void Visit(RStmt_StaticNotNullDirective& stmt) = 0;
    virtual void Visit(RStmt_StaticUnknownNullDirective& stmt) = 0;
};

class RStmt
{
public:
    virtual ~RStmt() { }
    virtual void Accept(RStmtVisitor& visitor) = 0;
};

class RStmt_Command : public RStmt
{
public:
    std::vector<RExp_String> commands;
public:
    IR0_API RStmt_Command(std::vector<RExp_String>&& commands);
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this);  }
};

// 로컬 변수는 
class RStmt_LocalVarDecl : public RStmt
{
public:
    RTypePtr type;
    std::string name;
    RExpPtr initExp;
public:
    IR0_API RStmt_LocalVarDecl();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_If : public RStmt
{
public:
    RExpPtr cond;
    std::vector<RStmtPtr> body;
    std::vector<RStmtPtr> elseBody;
public:
    IR0_API RStmt_If();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_IfNullableRefTest : public RStmt
{
public:
    RTypePtr refType;
    RName varName;
    RExpPtr asExp;
    std::vector<RStmtPtr> body;
    std::vector<RStmtPtr> elseBody;
public:
    IR0_API RStmt_IfNullableRefTest(RTypePtr&& refType, RName&& varName, RExpPtr&& asExp, std::vector<RStmtPtr>&& body, std::vector<RStmtPtr>&& elseBody);

    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_IfNullableValueTest : public RStmt
{
public:
    RTypePtr type;
    RName varName;
    RExpPtr asExp;
    std::vector<RStmtPtr> body;
    std::vector<RStmtPtr> elseBody;
public:
    IR0_API RStmt_IfNullableValueTest();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_For : public RStmt
{
public:
    std::vector<RStmtPtr> initStmts;
    RExpPtr condExp;
    RExpPtr continueExp;
    std::vector<RStmtPtr> body;
public:
    IR0_API RStmt_For(std::vector<RStmtPtr>&& initStmts, RExpPtr&& condExp, RExpPtr&& continueExp, std::vector<RStmtPtr>&& body);
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Continue : public RStmt
{
public:
    IR0_API RStmt_Continue();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Break : public RStmt
{
public:
    IR0_API RStmt_Break();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Return : public RStmt
{
public:
    RExpPtr exp;
public:
    IR0_API RStmt_Return(RExpPtr&& exp);
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Block : public RStmt
{
public:
    std::vector<RStmtPtr> stmts;
public:
    IR0_API RStmt_Block(std::vector<RStmtPtr>&& stmts);
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Blank : public RStmt
{
public:
    IR0_API RStmt_Blank();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Exp : public RStmt
{
public:
    RExpPtr exp;
public:
    IR0_API RStmt_Exp(RExpPtr&& exp);
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Task : public RStmt
{
public:
    std::shared_ptr<RLambdaDecl> lambdaDecl;
    std::vector<RArgument> captureArgs;
public:
    IR0_API RStmt_Task(std::shared_ptr<RLambdaDecl>&& lambdaDecl, std::vector<RArgument>&& captureArgs);
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Await : public RStmt
{
public:
    std::vector<RStmtPtr> body;
public:
    IR0_API RStmt_Await(std::vector<RStmtPtr>&& body);
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Async : public RStmt
{
public:
    std::shared_ptr<RLambdaDecl> lambdaDecl;
    std::vector<RArgument> captureArgs;
public:
    IR0_API RStmt_Async(std::shared_ptr<RLambdaDecl>&& lambdaDecl, std::vector<RArgument>&& captureArgs);
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Foreach : public RStmt
{
public:
    RTypePtr enumeratorType;
    RExpPtr enumeratorExp;
    RTypePtr itemType;
    RName varName;
    RExpPtr nextExp;
    std::vector<RStmtPtr> body;
public:
    IR0_API RStmt_Foreach();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_ForeachCast : public RStmt
{
public:
    RTypePtr enumeratorType;
    RExpPtr enumeratorExp;
    RTypePtr itemType;
    RName varName;
    RTypePtr rawItemType;
    RExpPtr nextExp;
    RExpPtr castExp;
    std::vector<RStmtPtr> body;
public:
    IR0_API RStmt_ForeachCast();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Yield : public RStmt
{
public:
    RExpPtr value;
public:
    IR0_API RStmt_Yield();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

// Constructor 내에서 상위 Constructor 호출시 사용
class RStmt_CallClassConstructor : public RStmt
{
public:
    std::shared_ptr<RClassConstructorDecl> constructor;
    std::vector<RArgument> args;
public:
    IR0_API RStmt_CallClassConstructor();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_CallStructConstructor : public RStmt
{
public:
    std::shared_ptr<RStructConstructorDecl> constructor;
    RTypeArgumentsPtr typeArgs;
    std::vector<RArgument> args;
public:
    IR0_API RStmt_CallStructConstructor();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_NullDirective : public RStmt
{
public:
    RLocPtr loc;
public:
    IR0_API RStmt_NullDirective();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_NotNullDirective : public RStmt
{
public:
    RLocPtr loc;
public:
    IR0_API RStmt_NotNullDirective();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_StaticNullDirective : public RStmt
{
public:
    RLocPtr loc;
public:
    IR0_API RStmt_StaticNullDirective();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_StaticNotNullDirective : public RStmt
{
public:
    RLocPtr loc;
public:
    IR0_API RStmt_StaticNotNullDirective();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_StaticUnknownNullDirective : public RStmt
{
public:
    RLocPtr loc;
public:
    IR0_API RStmt_StaticUnknownNullDirective();
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

}