#pragma once
#include <variant>
#include <vector>
#include "RType.h"
#include "RNames.h"
#include "RArgument.h"

namespace Citron {

class RCommandStmt;
class RLocalVarDeclStmt;
class RIfStmt;
class RIfNullableRefTestStmt;
class RIfNullableValueTestStmt;
class RForStmt;
class RContinueStmt;
class RBreakStmt;
class RReturnStmt;
class RBlockStmt;
class RBlankStmt;
class RExpStmt;
class RTaskStmt;
class RAwaitStmt;
class RAsyncStmt;
class RForeachStmt;
class RForeachCastStmt;
class RYieldStmt;
class RCallClassConstructorStmt;
class RCallStructConstructorStmt;
class RNullDirectiveStmt;
class RNotNullDirectiveStmt;
class RStaticNullDirectiveStmt;
class RStaticNotNullDirectiveStmt;
class RStaticUnknownNullDirectiveStmt;

class RLambdaDecl;
class RStructConstructorDecl;

class RStringExp;
class RExp;
class RLoc;
using RExpPtr = std::shared_ptr<RExp>;
using RStmtPtr = std::shared_ptr<RStmt>;
using RLocPtr = std::shared_ptr<RLoc>;

class RClassConstructor;

class RStmtVisitor
{
public:
    virtual void Visit(RCommandStmt& stmt) = 0;
    virtual void Visit(RLocalVarDeclStmt& stmt) = 0;
    virtual void Visit(RIfStmt& stmt) = 0;
    virtual void Visit(RIfNullableRefTestStmt& stmt) = 0;
    virtual void Visit(RIfNullableValueTestStmt& stmt) = 0;
    virtual void Visit(RForStmt& stmt) = 0;
    virtual void Visit(RContinueStmt& stmt) = 0;
    virtual void Visit(RBreakStmt& stmt) = 0;
    virtual void Visit(RReturnStmt& stmt) = 0;
    virtual void Visit(RBlockStmt& stmt) = 0;
    virtual void Visit(RBlankStmt& stmt) = 0;
    virtual void Visit(RExpStmt& stmt) = 0;
    virtual void Visit(RTaskStmt& stmt) = 0;
    virtual void Visit(RAwaitStmt& stmt) = 0;
    virtual void Visit(RAsyncStmt& stmt) = 0;
    virtual void Visit(RForeachStmt& stmt) = 0;
    virtual void Visit(RForeachCastStmt& stmt) = 0;
    virtual void Visit(RYieldStmt& stmt) = 0;
    virtual void Visit(RCallClassConstructorStmt& stmt) = 0;
    virtual void Visit(RCallStructConstructorStmt& stmt) = 0;
    virtual void Visit(RNullDirectiveStmt& stmt) = 0;
    virtual void Visit(RNotNullDirectiveStmt& stmt) = 0;
    virtual void Visit(RStaticNullDirectiveStmt& stmt) = 0;
    virtual void Visit(RStaticNotNullDirectiveStmt& stmt) = 0;
    virtual void Visit(RStaticUnknownNullDirectiveStmt& stmt) = 0;
};

class RStmt
{
public:
    virtual void Accept(RStmtVisitor& visitor) = 0;
};

class RCommandStmt : public RStmt
{
    std::vector<RStringExp> commands;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this);  }
};

// 로컬 변수는 
class RLocalVarDeclStmt : public RStmt
{
    RTypePtr type;
    std::string name;
    RExpPtr initExp;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RIfStmt : public RStmt
{
    RExpPtr cond;
    std::vector<RStmtPtr> body;
    std::vector<RStmtPtr> elseBody;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RIfNullableRefTestStmt : public RStmt
{
    RTypePtr refType;
    RName varName;
    RExpPtr asExp;
    std::vector<RStmtPtr> body;
    std::vector<RStmtPtr> elseBody;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RIfNullableValueTestStmt : public RStmt
{
    RTypePtr type;
    RName varName;
    RExpPtr asExp;
    std::vector<RStmtPtr> body;
    std::vector<RStmtPtr> elseBody;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RForStmt : public RStmt
{
    std::vector<RStmtPtr> initStmts;
    RExpPtr condExp;
    RExpPtr continueExp;
    std::vector<RStmtPtr> body;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RContinueStmt : public RStmt
{
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RBreakStmt : public RStmt
{
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RReturnStmt : public RStmt
{
    RExpPtr exp;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RBlockStmt : public RStmt
{
    std::vector<RStmt> stmts;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RBlankStmt : public RStmt
{
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RExpStmt : public RStmt
{
    RExpPtr exp;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RTaskStmt : public RStmt
{
    std::shared_ptr<RLambdaDecl> lambdaDecl;
    RTypeArgumentsPtr lambdaTypeArgs;

    std::vector<RArgument> captureArgs;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RAwaitStmt : public RStmt
{
    std::vector<RStmtPtr> body;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RAsyncStmt : public RStmt
{
    std::shared_ptr<RLambdaDecl> lambdaDecl;
    RTypeArgumentsPtr lambdaTypeArgs;
    std::vector<RArgument> captureArgs;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RForeachStmt : public RStmt
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

class RForeachCastStmt : public RStmt
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

class RYieldStmt : public RStmt
{
    RExpPtr value;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

// Constructor 내에서 상위 Constructor 호출시 사용
class RCallClassConstructorStmt : public RStmt
{
    std::shared_ptr<RClassConstructor> constructor;
    std::vector<RArgument> args;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RCallStructConstructorStmt : public RStmt
{
    std::shared_ptr<RStructConstructorDecl> constructor;
    RTypeArgumentsPtr typeArgs;
    std::vector<RArgument> args;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RNullDirectiveStmt : public RStmt
{
    RLocPtr loc;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RNotNullDirectiveStmt : public RStmt
{
    RLocPtr loc;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStaticNullDirectiveStmt : public RStmt
{
    RLocPtr loc;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStaticNotNullDirectiveStmt : public RStmt
{
    RLocPtr loc;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

class RStaticUnknownNullDirectiveStmt : public RStmt
{
    RLocPtr loc;
public:
    void Accept(RStmtVisitor& visitor) override { visitor.Visit(*this); }
};

}