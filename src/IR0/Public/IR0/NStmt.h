#pragma once

#include "IR0Config.h"

#include <variant>
#include <vector>
#include <string>
#include <memory>

#include "RNames.h"
#include "NArgument.h"

namespace Citron {

class RType;
class RTypeArguments;

class RClassCtorDecl;

class NExp_String;
class NLambdaDecl;
class NStructCtorDecl;

class NLoc;

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

class NStmtVisitor
{
public:
    virtual ~NStmtVisitor() {}
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
    virtual ~NStmt() {}
    virtual void Accept(NStmtVisitor& visitor) = 0;
};

class NStmt_Command : public NStmt
{
public:
    std::vector<NExp_String*> commands;
public:
    IR0_API NStmt_Command(std::vector<NExp_String*>&& commands);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

// 로컬 변수는 
class NStmt_LocalVarDecl : public NStmt
{
public:
    RType* type;
    std::string name;
    NExp* initExp;
public:
    IR0_API NStmt_LocalVarDecl(RType* type, const std::string& name, NExp* initExp);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_If : public NStmt
{
public:
    NExp* cond;
    std::vector<NStmt*> body;
    std::vector<NStmt*> elseBody;
public:
    IR0_API NStmt_If(NExp* cond, std::vector<NStmt*>&& body, std::vector<NStmt*>&& elseBody);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_IfNullableRefTest : public NStmt
{
public:
    RType* refType;
    RName varName;
    NExp* asExp;
    std::vector<NStmt*> body;
    std::vector<NStmt*> elseBody;
public:
    IR0_API NStmt_IfNullableRefTest(RType* refType, RName&& varName, NExp* asExp, std::vector<NStmt*>&& body, std::vector<NStmt*>&& elseBody);

    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_IfNullableValueTest : public NStmt
{
public:
    RType* type;
    RName varName;
    NExp* asExp;
    std::vector<NStmt*> body;
    std::vector<NStmt*> elseBody;
public:
    IR0_API NStmt_IfNullableValueTest(RType* type, RName&& varName, NExp* asExp, std::vector<NStmt*>&& body, std::vector<NStmt*>&& elseBody);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_For : public NStmt
{
public:
    std::vector<NStmt*> initStmts;
    NExp* condExp;
    NExp* continueExp;
    std::vector<NStmt*> body;
public:
    IR0_API NStmt_For(std::vector<NStmt*>&& initStmts, NExp* condExp, NExp* continueExp, std::vector<NStmt*>&& body);
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
    NExp* exp;
public:
    IR0_API NStmt_Return(NExp* exp);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Block : public NStmt
{
public:
    std::vector<NStmt*> stmts;
public:
    IR0_API NStmt_Block(std::vector<NStmt*>&& stmts);
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
    NExp* exp;
public:
    IR0_API NStmt_Exp(NExp* exp);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Task : public NStmt
{
public:
    NLambdaDecl* lambdaDecl;
    std::vector<NArgument> captureArgs;
public:
    IR0_API NStmt_Task(NLambdaDecl* lambdaDecl, std::vector<NArgument>&& captureArgs);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Await : public NStmt
{
public:
    std::vector<NStmt*> body;
public:
    IR0_API NStmt_Await(std::vector<NStmt*>&& body);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Async : public NStmt
{
public:
    NLambdaDecl* lambdaDecl;
    std::vector<NArgument> captureArgs;
public:
    IR0_API NStmt_Async(NLambdaDecl* lambdaDecl, std::vector<NArgument>&& captureArgs);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Foreach : public NStmt
{
public:
    NExp* enumeratorExp;
    RType* itemType;
    RName varName;
    NExp* nextExp;
    std::vector<NStmt*> body;
public:
    IR0_API NStmt_Foreach(NExp* enumeratorExp, RType* itemType, const RName& varName, NExp* nextExp, std::vector<NStmt*>&& body);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_ForeachCast : public NStmt
{
public:
    NExp* enumeratorExp;
    RType* itemType;
    RName varName;
    RType* rawItemType;
    NExp* nextExp;
    NExp* castExp;
    std::vector<NStmt*> body;
public:
    IR0_API NStmt_ForeachCast(NExp* enumeratorExp, RType* itemType, const RName& varName, RType* rawItemType, NExp* nextExp, NExp* castExp, std::vector<NStmt*>&& body);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_Yield : public NStmt
{
public:
    NExp* value;
public:
    IR0_API NStmt_Yield(NExp* value);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

// Ctor 내에서 상위 Ctor 호출시 사용
class NStmt_CallClassCtor : public NStmt
{
public:
    RClassCtorDecl* ctor;
    std::vector<NArgument> args;
public:
    IR0_API NStmt_CallClassCtor();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_CallStructCtor : public NStmt
{
public:
    NStructCtorDecl* ctor;
    RTypeArguments* typeArgs;
    std::vector<NArgument> args;
public:
    IR0_API NStmt_CallStructCtor();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_NullDirective : public NStmt
{
public:
    NLoc* loc;
public:
    IR0_API NStmt_NullDirective(NLoc* loc);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_NotNullDirective : public NStmt
{
public:
    NLoc* loc;
public:
    IR0_API NStmt_NotNullDirective(NLoc* loc);
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_StaticNullDirective : public NStmt
{
public:
    NLoc* loc;
public:
    IR0_API NStmt_StaticNullDirective();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_StaticNotNullDirective : public NStmt
{
public:
    NLoc* loc;
public:
    IR0_API NStmt_StaticNotNullDirective();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class NStmt_StaticUnknownNullDirective : public NStmt
{
public:
    NLoc* loc;
public:
    IR0_API NStmt_StaticUnknownNullDirective();
    void Accept(NStmtVisitor& visitor) override { visitor.Visit(*this); }
};

}