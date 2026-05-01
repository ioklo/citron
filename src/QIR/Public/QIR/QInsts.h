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
    QArg_Addr _this;
    std::string text; // TODO: ptr이 들어가는 slot으로 바꾸고, Global Ptr을 넣는 방식으로 바꾼 다음, Intrinsic으로 넣기. QData에는 GlobalString을 넣고, index로 참조하기
};

// %dest = load [%src]
struct QInst_Load
{
    RType* type;
    QArg_Dest dest;       // T slot
    QArg_Addr_PtrSlot src; // T* 나타내는 slot가능
};

// store [%dest], %src
// dest는 ptr을 담고 있음
struct QInst_Store
{
    RType* type;     // T
    QArg_Addr_PtrSlot dest;   // T*을 나타내는 slot, QArg_Addr_OfSlot은 그냥 Slot에 Assign을 하면 된다
    QArg_Value src;   // T의 const, register 가능
};

// slot의 위치 포인터를 반환한다
struct QInst_AddrOf
{
    QArg_Dest dest;
    size_t slot;
};

struct QInst_FieldOf
{
    QArg_Dest dest;    // ptr
    QArg_Addr src;      // ptr
    size_t fieldIndex;
};

// %dest = <ty> %src
struct QInst_Assign
{
    RType* type;
    QArg_Dest dest; // T slot 가능
    QArg_Value src;  // T const, slot가능
};

// class, struct, interface 구분 없이 Call
struct QInst_Call
{   
    RFuncDecl* rFuncDecl;
    std::optional<QArg_Dest> o_dest;      // Return Passing Mode가 direct인 경우 사용한다. 리턴값이 void거나 indirect인 경우에는 nullopt
    std::vector<QArg_CallArg> args;         // param Passing Mode에 따라서 Direct인 경우 값에 해당하는 Slot, const가 들어가고, Indirect인 경우 포인터에 해당하는 Slot이 들어간다.
                                          // 순서대로 indirect return, this, 나머지 인자들이 들어간다
};

struct QInst_Return
{   
};

enum struct QInst_IntrinsicKind
{   
    Command_Item, // Command Items 였는데, list가 아직 안만들어져서 Command Item으로 변경
    Alloc_Int,
    Memcpy_Void_Ptr_Ptr_Int,

    NewList_Items,
    GetIterator_ListPtr_ListIterator,
    LogicalNot_Bool_Bool,
    UnaryMinus_Int_Int,
    ToString_String_Bool,
    ToString_String_Int,

    PrefixInc_Int_IntRef,
    PrefixDec_Int_IntRef,
    PostfixInc_Int_IntRef,
    PostfixDec_Int_IntRef,

    Multiply_Int_Int_Int,
    Divide_Int_Int_Int,
    Modulo_Int_Int_Int,
    Add_Int_Int_Int,
    Add_String_StringInRef_StringInRef,
    Subtract_Int_Int_Int,
    LessThan_Bool_Int_Int,
    LessThan_Bool_StringInRef_StringInRef,
    GreaterThan_Bool_Int_Int,
    GreaterThan_Bool_StringInRef_StringInRef,
    LessThanOrEqual_Bool_Int_Int,
    LessThanOrEqual_Bool_StringInRef_StringInRef,
    GreaterThanOrEqual_Bool_Int_Int,
    GreaterThanOrEqual_Bool_StringInRef_StringInRef,
    Equal_Bool_Int_Int,
    Equal_Bool_Bool_Bool,
    Equal_Bool_StringInRef_StringInRef,

    CopyCtor_Void_StringRef_StringInRef,
    MoveCtor_Void_StringRef_StringMoveRef,
    Dtor_Void_StringRef,
    CopyAssign_Void_StringRef_StringInRef,
    MoveAssign_Void_StringRef_StringMoveRef,

    Max
};

struct QInst_Intrinsic
{   
    QInst_IntrinsicKind kind;
    std::optional<QArg_Dest> o_dest;
    std::vector<QArg_CallArg> args;

    QInst_Intrinsic(QInst_IntrinsicKind kind, std::optional<QArg_Dest>&& o_dest, std::vector<QArg_CallArg>&& args)
        : kind{kind}, o_dest{std::move(o_dest)}, args{std::move(args)}
    {
    }
};

struct QInst_CondJump
{
    QArg_Value_Slot cond; // 여기에 const를 쓸거면 CondJump를 뭣하러 하는가. 그냥 Slot만 받도록 한다
    QBlock* trueBlock;
    QBlock* falseBlock;
};

struct QInst_Jump
{
    QBlock* block;
};

using QInst = std::variant<
    QInst_Ctor_String,
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