#pragma once
#include <string>
#include <variant>
#include <optional>

#include "QValues.h"

namespace Citron {

class RFuncDecl;
class QBlock;

// QLocalVar(lv, name)
struct QInst_LocalVarDecl
{
    QValue loc;
    std::string name;
};

// QInst_Store(lv, v)
struct QInst_Store
{
    QValue loc;
    QValue value;
};

// QInst_Load(v, lv)
struct QInst_Load
{
    QValue value;
    QValue loc;
};

struct QInst_Alloc
{
    QValue loc;
    size_t size;
};

// class, struct, interface 구분 없이 Call
struct QInst_Call
{
    RFuncDecl* funcDecl;
    std::vector<QValue> args;
};

struct QInst_ReturnVoid
{
};

enum struct QInst_IntrinsicKind
{   
    DebugPrint_Items,
    Command_Items,

    NewList_Items,
    GetListIterator_List,
    LogicalNot_Bool,
    UnaryMinus_Int,
    ToString_Bool,
    ToString_Int,

    PrefixInc_Int,
    PrefixDec_Int,
    PostfixInc_Int,
    PostfixDec_Int,

    Multiply_Int_Int,
    Divide_Int_Int,
    Modulo_Int_Int,
    Add_Int_Int,
    Add_String_String,
    Subtract_Int_Int,
    LessThan_Int_Int,
    LessThan_String_String,
    GreaterThan_Int_Int,
    GreaterThan_String_String,
    LessThanOrEqual_Int_Int,
    LessThanOrEqual_String_String,
    GreaterThanOrEqual_Int_Int,
    GreaterThanOrEqual_String_String,
    Equal_Int_Int,
    Equal_Bool_Bool,
    Equal_String_String,
};

struct QInst_Intrinsic
{
    QInst_IntrinsicKind kind;
    std::optional<QValue_Named> result;
    std::vector<QValue> args;

    QInst_Intrinsic(QInst_IntrinsicKind kind, std::optional<QValue_Named>&& result, std::vector<QValue>&& args)
        : kind{kind}, result{std::move(result)}, args{std::move(args)}
    {
    }
};

struct QInst_CondJump
{
    QValue value;
    QBlock* trueBlock;
    QBlock* falseBlock;
};

struct QInst_Jump
{
    QBlock* block;
};

using QInst = std::variant<
    QInst_LocalVarDecl,
    QInst_Store,
    QInst_Load,
    QInst_Alloc,
    QInst_Call,
    QInst_Intrinsic,
    QInst_CondJump,
    QInst_Jump,
    QInst_ReturnVoid
>;

using QTermInst = std::variant<
    QInst_Jump,
    QInst_CondJump, // 완전 CondJump
    QInst_ReturnVoid
>;

} // namespace Citron