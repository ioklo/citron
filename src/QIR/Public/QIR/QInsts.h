#pragma once
#include <string>
#include <variant>
#include <optional>

#include "QArgs.h"

namespace Citron {

class RFuncDecl;
class QBlock;

// init_string %dest, "hello"
struct QInst_InitString
{
    QArg_Slot dest;
    std::string text;
};

// %dest = load [%src]
struct QInst_Load
{
    QType *qType;
    QArg_Slot dest;  // T slot
    QArg_Slot src;   // T* 나타내는 slot가능
};

// store [%dest], %src
// loc은 ptr을 담고 있음
struct QInst_Store
{
    QType* qType;     // T
    QArg_Slot dest;   // T*을 나타내는 slot
    QArg_Input src;   // T의 const, register 가능
};

// slot의 위치 포인터를 반환한다
struct QInst_AddrOf
{
    QArg_Slot dest;
    QArg_Slot slot;
};

// %dest = <ty> %src
struct QInst_Assign
{
    QType* qType;
    QArg_Slot dest; // T slot 가능
    QArg_Input src;  // T const, slot가능
};

// class, struct, interface 구분 없이 Call
struct QInst_Call
{   
    RFuncDecl* rFuncDecl;
    std::optional<QArg_Slot> oDest;      // void인 경우 nullopt, 나머지는 slot
    std::vector<QArg_Input> args;         // 
};

struct QInst_ReturnValue
{
    QType* qType;
    QArg_Input value;
};

struct QInst_Return
{
    std::optional<QInst_ReturnValue> oValue; // void면 없음
};

enum struct QInst_IntrinsicKind
{   
    DebugPrint_Items,
    Command_Items,
    Alloc_Int,
    Memcpy_Ptr_Ptr_Int,

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
    std::optional<QArg_Slot> oDest; // stack slot인 경우 첫 arg에 들어간다
    std::vector<QArg_Input> args;

    QInst_Intrinsic(QInst_IntrinsicKind kind, std::optional<QArg_Slot>&& oDest, std::vector<QArg_Input>&& args)
        : kind{kind}, oDest{std::move(oDest)}, args{std::move(args)}
    {
    }
};

struct QInst_CondJump
{
    QArg_Slot cond; // 여기에 const를 쓸거면 CondJump를 뭣하러 하는가. 그냥 Slot만 받도록 한다
    QBlock* trueBlock;
    QBlock* falseBlock;
};

struct QInst_Jump
{
    QBlock* block;
};

using QInst = std::variant<
    QInst_InitString,
    QInst_Load,
    QInst_Store,
    QInst_AddrOf,
    QInst_Assign,
    QInst_Call,
    QInst_Return,
    QInst_Intrinsic,
    QInst_CondJump,
    QInst_Jump
>;

using QTermInst = std::variant<
    QInst_Jump,
    QInst_CondJump, // 완전 CondJump
    QInst_Return
>;

} // namespace Citron