#pragma once
#include <variant>
#include <vector>

namespace Citron {

using RStmt = std::variant<
    class RCommandStmt,
    class RLocalVarDeclStmt,
    class RIfStmt,
    class RIfNullableRefTestStmt,
    class RIfNullableValueTestStmt,
    class RForStmt,
    class RContinueStmt,
    class RBreakStmt,
    class RReturnStmt,
    class RBlockStmt,
    class RBlankStmt,
    class RExpStmt,
    class RTaskStmt,
    class RAwaitStmt,
    class RAsyncStmt,
    class RForeachStmt,
    class RForeachCastStmt,
    class RYieldStmt,
    class RCallClassConstructorStmt,
    class RCallStructConstructorStmt,

    class RNullDirectiveStmt,
    class RNotNullDirectiveStmt,
    class RStaticNullDirectiveStmt,
    class RStaticNotNullDirectiveStmt,
    class RStaticUnknownNullDirectiveStmt
>;

class RCommandStmt
{
    std::vector<RStringExp> commands;
};

// 로컬 변수는 
class RLocalVarDeclStmt
{
    MIType type;
    string name;
    std::optional<RExp> initExp;
};

class RIfStmt
{
    RExp cond;
    std::vector<RStmt> body;
    std::vector<RStmt> elseBody;
};

class RIfNullableRefTestStmt
{
    MType refType;
    MName varName;
    RExp asExp;
    std::vector<RStmt> body;
    std::vector<RStmt> elseBody;
};

class RIfNullableValueTestStmt
{
    MType type;
    MName varName;
    RExp asExp;
    std::vector<RStmt> body;
    std::vector<RStmt> elseBody;
};

class RForStmt
{
    std::vector<RStmt> initStmts;
    std::optional<RExp> condExp;
    std::optional<RExp> continueExp;
    std::vector<RStmt> body;
};

class RContinueStmt
{
};

class RBreakStmt
{
};

class RReturnStmt
{
    std::optional<RExp> exp;
}

class RBlockStmt
{
    std::vector<RStmt> stmts;
};

class RBlankStmt
{
};

class RExpStmt
{
    RExp exp;
};

class RTaskStmt
{
    MLambda lambda;
    std::vector<RArgument> captureArgs;
};

class RAwaitStmt
{
    std::vector<RStmt> body;
};

class RAsyncStmt
{
    MLambda lambda;
    std::vector<Argument> captureArgs;
};

class RForeachStmt
{
    MType enumeratorType;
    RExp enumeratorExp;
    MType itemType;
    MName varName;
    RExp nextExp;
    std::vector<RStmt> body;
};

class RForeachCastStmt
{
    MType enumeratorType;
    RExp enumeratorExp;
    MType itemType;
    MName varName;
    MType rawItemType;
    RExp nextExp;
    RExp castExp;
    std::vector<RStmt> body;
};

class RYieldStmt
{
    RExp value;
};

// Constructor 내에서 상위 Constructor 호출시 사용
class RCallClassConstructorStmt
{
    MClassConstructor constructor;
    std::vector<Argument> args;
};

class RCallStructConstructorStmt
{
    MStructConstructor constructor;
    std::vector<Argument> args;
};

class RNullDirectiveStmt
{
    RLoc loc;
};

class RNotNullDirectiveStmt
{
    RLoc loc;
};

class RStaticNullDirectiveStmt
{
    RLoc loc;
};

class RStaticNotNullDirectiveStmt
{
    RLoc loc;
};

class RStaticUnknownNullDirectiveStmt
{
    RLoc loc;
};

}