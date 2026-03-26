#pragma once
#include <string>
#include <variant>
#include <optional>

#include "QArgs.h"

namespace Citron {

class RType;
class RFuncDecl;
class QBlock;

// construct <string>, %slot, "hello"
struct QInst_Ctor_String
{
    QArg_Slot slot;
    std::string text; // TODO: ptr이 들어가는 slot으로 바꾸고, Global Ptr을 넣는 방식으로 바꾼 다음, Intrinsic으로 넣기. QData에는 GlobalString을 넣고, index로 참조하기
};

struct QInst_CopyCtor_String
{
    QArg_Slot slot;
    QArg_Slot src;
};

struct QInst_MoveCtor_String
{
    QArg_Slot slot;
    QArg_Slot src;
};

struct QInst_Dtor_String
{
    QArg_Slot slot; // 
};

// copy assign
struct QInst_CopyAssign_String
{
    QArg_Slot dest;
    QArg_Slot src;
};

// move assign
struct QInst_MoveAssign_String
{
    QArg_Slot dest;
    QArg_Slot src;
};

// %dest = load [%src]
struct QInst_Load
{
    RType* type;
    QArg_Slot dest;  // T slot
    QArg_Slot src;   // T* 나타내는 slot가능
};

// store [%dest], %src
// dest는 ptr을 담고 있음
struct QInst_Store
{
    RType* type;     // T
    QArg_Slot dest;   // T*을 나타내는 slot
    QArg_Input src;   // T의 const, register 가능
};

// slot의 위치 포인터를 반환한다
struct QInst_AddrOf
{
    QArg_Slot dest;
    QArg_Slot slot;
};

struct QInst_FieldOf
{
    QArg_Slot dest;    // ptr
    QArg_Slot src;     // ptr
    size_t fieldIndex;
};

// %dest = <ty> %src
struct QInst_Assign
{
    RType* type;
    QArg_Slot dest; // T slot 가능
    QArg_Input src;  // T const, slot가능
};

// class, struct, interface 구분 없이 Call
struct QInst_Call
{   
    RFuncDecl* rFuncDecl;
    std::optional<QArg_Slot> o_dest;      // void인 경우 nullopt, 나머지는 slot
    std::vector<QArg_Input> args;         // 
};

struct QInst_ReturnValue
{
    RType* type;
    QArg_Input value;
};

struct QInst_Return
{
    std::optional<QInst_ReturnValue> o_value; // void면 없음
};

enum struct QInst_IntrinsicKind
{   
    Command_Items,
    Alloc_Int,
    Memcpy_Ptr_Ptr_Int,

    NewList_Items,
    GetIterator_ListPtr_ListIterator,
    LogicalNot_Bool_Bool,
    UnaryMinus_Int_Int,
    ToString_Bool_String,
    ToString_Int_String,

    PrefixInc_Int_Int,
    PrefixDec_Int_Int,
    PostfixInc_Int_Int,
    PostfixDec_Int_Int,

    Multiply_Int_Int_Int,
    Divide_Int_Int_Int,
    Modulo_Int_Int_Int,
    Add_Int_Int_Int,
    Add_StringPtr_StringPtr_String,
    Subtract_Int_Int_Int,
    LessThan_Int_Int_Bool,
    LessThan_StringPtr_StringPtr_Bool,
    GreaterThan_Int_Int_Bool,
    GreaterThan_StringPtr_StringPtr_Bool,
    LessThanOrEqual_Int_Int_Bool,
    LessThanOrEqual_StringPtr_StringPtr_Bool,
    GreaterThanOrEqual_Int_Int_Bool,
    GreaterThanOrEqual_StringPtr_StringPtr_Bool,
    Equal_Int_Int_Bool,
    Equal_Bool_Bool_Bool,
    Equal_StringPtr_StringPtr_Bool,
};

struct QInst_Intrinsic
{   
    QInst_IntrinsicKind kind;
    std::optional<QArg_Slot> o_dest;
    std::vector<QArg_Input> args;

    QInst_Intrinsic(QInst_IntrinsicKind kind, std::optional<QArg_Slot>&& o_dest, std::vector<QArg_Input>&& args)
        : kind{kind}, o_dest{std::move(o_dest)}, args{std::move(args)}
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
    QInst_Ctor_String,
    QInst_CopyCtor_String,
    QInst_MoveCtor_String,
    QInst_CopyAssign_String,
    QInst_MoveAssign_String,
    QInst_Dtor_String,
    QInst_Load,
    QInst_Store,
    QInst_AddrOf,
    QInst_FieldOf,
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