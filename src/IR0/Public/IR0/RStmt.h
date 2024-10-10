#pragma once
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

class RExp_String;
class RExp;
class RLoc;
using RExpPtr = std::shared_ptr<RExp>;
using RStmtPtr = std::shared_ptr<RStmt>;
using RLocPtr = std::shared_ptr<RLoc>;

class RClassConstructor;

class RStmtVisitor
{
public:
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
    virtual void Accept(RStmtVisitor& visitor) = 0;
};

class RStmt_Command : public RStmt
{
    std::vector<RExp_String> commands;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this);  }
};

// 로컬 변수는 
class RStmt_LocalVarDecl : public RStmt
{
    RTypePtr type;
    std::string name;
    RExpPtr initExp;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_If : public RStmt
{
    RExpPtr cond;
    std::vector<RStmtPtr> body;
    std::vector<RStmtPtr> elseBody;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_IfNullableRefTest : public RStmt
{
    RTypePtr refType;
    RName varName;
    RExpPtr asExp;
    std::vector<RStmtPtr> body;
    std::vector<RStmtPtr> elseBody;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_IfNullableValueTest : public RStmt
{
    RTypePtr type;
    RName varName;
    RExpPtr asExp;
    std::vector<RStmtPtr> body;
    std::vector<RStmtPtr> elseBody;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_For : public RStmt
{
    std::vector<RStmtPtr> initStmts;
    RExpPtr condExp;
    RExpPtr continueExp;
    std::vector<RStmtPtr> body;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Continue : public RStmt
{
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Break : public RStmt
{
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Return : public RStmt
{
    RExpPtr exp;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Block : public RStmt
{
    std::vector<RStmt> stmts;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Blank : public RStmt
{
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Exp : public RStmt
{
    RExpPtr exp;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Task : public RStmt
{
    std::shared_ptr<RLambdaDecl> lambdaDecl;
    RTypeArgumentsPtr lambdaTypeArgs;

    std::vector<RArgument> captureArgs;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Await : public RStmt
{
    std::vector<RStmtPtr> body;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Async : public RStmt
{
    std::shared_ptr<RLambdaDecl> lambdaDecl;
    RTypeArgumentsPtr lambdaTypeArgs;
    std::vector<RArgument> captureArgs;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Foreach : public RStmt
{
    RTypePtr enumeratorType;
    RExpPtr enumeratorExp;
    RTypePtr itemType;
    RName varName;
    RExpPtr nextExp;
    std::vector<RStmtPtr> body;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_ForeachCast : public RStmt
{
    RTypePtr enumeratorType;
    RExpPtr enumeratorExp;
    RTypePtr itemType;
    RName varName;
    RTypePtr rawItemType;
    RExpPtr nextExp;
    RExpPtr castExp;
    std::vector<RStmtPtr> body;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_Yield : public RStmt
{
    RExpPtr value;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

// Constructor 내에서 상위 Constructor 호출시 사용
class RStmt_CallClassConstructor : public RStmt
{
    std::shared_ptr<RClassConstructor> constructor;
    std::vector<RArgument> args;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_CallStructConstructor : public RStmt
{
    std::shared_ptr<RStructConstructorDecl> constructor;
    RTypeArgumentsPtr typeArgs;
    std::vector<RArgument> args;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_NullDirective : public RStmt
{
    RLocPtr loc;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_NotNullDirective : public RStmt
{
    RLocPtr loc;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_StaticNullDirective : public RStmt
{
    RLocPtr loc;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_StaticNotNullDirective : public RStmt
{
    RLocPtr loc;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStmt_StaticUnknownNullDirective : public RStmt
{
    RLocPtr loc;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

}