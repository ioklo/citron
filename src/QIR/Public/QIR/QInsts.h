#pragma once
#include <string>
#include <variant>
#include <optional>

#include "QArgs.h"

namespace Citron {

class RFuncDecl;
class QBlock;

// %slot = init_string "hello"
struct QInst_InitString
{
    QArg_StackSlot slot;
    std::string text;
};

// store [%dest], %value
// loc은 ptr을 담고 있음
struct QInst_Store
{
    QArg dest;   // T*을 나타내는 register, slot 가능
    QArg value;  // T의 const, register, slot 가능
};

// %value = load [%src]
struct QInst_Load
{
    QArg value; // T register, slot 가능
    QArg src;   // T* 나타내는 register, slot가능
};

// %dest = %src
struct QInst_Assign
{
    QArg dest; // T register, slot 가능
    QArg src;  // T const, register, slot 가능
    size_t size;
};

// class, struct, interface 구분 없이 Call
struct QInst_Call
{
    RFuncDecl* funcDecl;
    std::vector<QArg> args;
};

struct QInst_ReturnVoid
{
};

enum struct QInst_IntrinsicKind
{   
    DebugPrint_Items,
    Command_Items,
    Alloc_Int,

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
    std::optional<QArg> result;
    std::vector<QArg> args;

    QInst_Intrinsic(QInst_IntrinsicKind kind, std::optional<QArg>&& result, std::vector<QArg>&& args)
        : kind{kind}, result{std::move(result)}, args{std::move(args)}
    {
    }
};

struct QInst_CondJump
{
    QArg cond;
    QBlock* trueBlock;
    QBlock* falseBlock;
};

struct QInst_Jump
{
    QBlock* block;
};

using QInst = std::variant<
    QInst_InitString,
    QInst_Store,
    QInst_Load,
    QInst_Assign,
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