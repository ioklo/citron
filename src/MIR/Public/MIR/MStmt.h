#pragma once

#include "MIRConfig.h"

#include <variant>
#include <vector>
#include <string>
#include <optional>

#include "RSymbol/RNames.h"
#include "MArgument.h"

namespace Citron {

class RType;
class RTypeArguments;

class RClassCtorDecl;
class RStructCtorDecl;

class MExp_String;
class NLambdaDecl;
class NStructCtorDecl;

class MLoc;

struct MStmtVisitor;
class MStmt
{
public:
    virtual ~MStmt() {}
    virtual void Accept(MStmtVisitor& visitor) = 0;
};

class MStmt_Command : public MStmt
{
public:
    std::vector<MExp_String*> commands;
public:
    MIR_API MStmt_Command(std::vector<MExp_String*>&& commands);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_LocalVarDeclInit_Uninit {};
struct MStmt_LocalVarDeclInit_Create { MCreate create; };

using MStmt_LocalVarDeclInit = std::variant<MStmt_LocalVarDeclInit_Uninit, MStmt_LocalVarDeclInit_Create>;

// 로컬 변수는 
class MStmt_LocalVarDecl : public MStmt
{
public:
    RType* type;
    RName name;
    MStmt_LocalVarDeclInit init;

public:
    MIR_API MStmt_LocalVarDecl(RType* type, const RName& name, MStmt_LocalVarDeclInit init);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_LocalRefDecl : public MStmt
{
public:
    RType* type;
    RName name;
    MLoc* loc;

public:
    MIR_API MStmt_LocalRefDecl(RType* type, RName&& name, MLoc* loc);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_If : public MStmt
{
public:
    MExp* cond;
    std::vector<MStmt*> body;
    std::vector<MStmt*> elseBody;
public:
    MIR_API MStmt_If(MExp* cond, std::vector<MStmt*>&& body, std::vector<MStmt*>&& elseBody);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_IfNullableRefTest : public MStmt
{
public:
    RType* refType;
    RName varName;
    MExp* asExp;
    std::vector<MStmt*> body;
    std::vector<MStmt*> elseBody;
public:
    MIR_API MStmt_IfNullableRefTest(RType* refType, RName&& varName, MExp* asExp, std::vector<MStmt*>&& body, std::vector<MStmt*>&& elseBody);

    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_IfNullableValueTest : public MStmt
{
public:
    RType* type;
    RName varName;
    MExp* asExp;
    std::vector<MStmt*> body;
    std::vector<MStmt*> elseBody;
public:
    MIR_API MStmt_IfNullableValueTest(RType* type, RName&& varName, MExp* asExp, std::vector<MStmt*>&& body, std::vector<MStmt*>&& elseBody);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_For : public MStmt
{
public:
    std::vector<MStmt*> initStmts;
    MExp* condExp;
    MExp* continueExp;
    std::vector<MStmt*> body;
public:
    MIR_API MStmt_For(std::vector<MStmt*>&& initStmts, MExp* condExp, MExp* continueExp, std::vector<MStmt*>&& body);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_Continue : public MStmt
{
public:
    MIR_API MStmt_Continue();
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_Break : public MStmt
{
public:
    MIR_API MStmt_Break();
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_Return : public MStmt
{
public:
    MExp* exp;
public:
    MIR_API MStmt_Return(MExp* exp);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_Block : public MStmt
{
public:
    std::vector<MStmt*> stmts;
public:
    MIR_API MStmt_Block(std::vector<MStmt*>&& stmts);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_Blank : public MStmt
{
public:
    MIR_API MStmt_Blank();
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_Exp : public MStmt
{
public:
    MExp* exp;
public:
    MIR_API MStmt_Exp(MExp* exp);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_Task : public MStmt
{
public:
    NLambdaDecl* lambdaDecl;
    std::vector<MArgument> captureArgs;
public:
    MIR_API MStmt_Task(NLambdaDecl* lambdaDecl, std::vector<MArgument>&& captureArgs);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_Await : public MStmt
{
public:
    std::vector<MStmt*> body;
public:
    MIR_API MStmt_Await(std::vector<MStmt*>&& body);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_Async : public MStmt
{
public:
    NLambdaDecl* lambdaDecl;
    std::vector<MArgument> captureArgs;
public:
    MIR_API MStmt_Async(NLambdaDecl* lambdaDecl, std::vector<MArgument>&& captureArgs);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_Foreach : public MStmt
{
public:
    MExp* enumeratorExp;
    RType* itemType;
    RName varName;
    MExp* nextExp;
    std::vector<MStmt*> body;
public:
    MIR_API MStmt_Foreach(MExp* enumeratorExp, RType* itemType, const RName& varName, MExp* nextExp, std::vector<MStmt*>&& body);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_ForeachCast : public MStmt
{
public:
    MExp* enumeratorExp;
    RType* itemType;
    RName varName;
    RType* rawItemType;
    MExp* nextExp;
    MExp* castExp;
    std::vector<MStmt*> body;
public:
    MIR_API MStmt_ForeachCast(MExp* enumeratorExp, RType* itemType, const RName& varName, RType* rawItemType, MExp* nextExp, MExp* castExp, std::vector<MStmt*>&& body);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_Yield : public MStmt
{
public:
    MExp* value;
public:
    MIR_API MStmt_Yield(MExp* value);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

// Ctor 내에서 상위 Ctor 호출시 사용
class MStmt_CallClassCtor : public MStmt
{
public:
    RClassCtorDecl* ctor;
    std::vector<MArgument> args;
public:
    MIR_API MStmt_CallClassCtor(RClassCtorDecl* ctor, std::vector<MArgument>&& args);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_CallStructCtor : public MStmt
{
public:
    NStructCtorDecl* ctor;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;
public:
    MIR_API MStmt_CallStructCtor(NStructCtorDecl* ctor, RTypeArguments* typeArgs, std::vector<MArgument>&& args);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_NullDirective : public MStmt
{
public:
    MLoc* loc;
public:
    MIR_API MStmt_NullDirective(MLoc* loc);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_NotNullDirective : public MStmt
{
public:
    MLoc* loc;
public:
    MIR_API MStmt_NotNullDirective(MLoc* loc);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_StaticNullDirective : public MStmt
{
public:
    MLoc* loc;
public:
    MIR_API MStmt_StaticNullDirective(MLoc* loc);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_StaticNotNullDirective : public MStmt
{
public:
    MLoc* loc;
public:
    MIR_API MStmt_StaticNotNullDirective(MLoc* loc);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

class MStmt_StaticUnknownNullDirective : public MStmt
{
public:
    MLoc* loc;
public:
    MIR_API MStmt_StaticUnknownNullDirective(MLoc* loc);
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

}

#include "MStmtVisitor.g.h"

