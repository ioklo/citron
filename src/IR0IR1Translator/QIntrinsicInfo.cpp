#include "QIntrinsicInfo.h"
#include <unordered_map>
#include <cassert>
#include "RSymbol/RFactory.h"
#include "MIR/MExp.h"
#include "MIR/MInitExp.h"
#include "QIR/QInsts.h"
#include "QTranslationContexts.h"

using namespace std;

namespace Citron {

namespace {

RFuncParameter StringParam()
{
}

//template<typename... TArgs>
//QIntrinsicInfo Make(QInst_IntrinsicKind kind, RType* retType, TArgs&&... args)
//{
//    return {.kind = kind, .funcRet = RFuncReturn(retType), .funcParams{std::forward<TArgs>(args)...}};
//}
}

QIntrinsicInfo* GetIntrinsicInfo(QInst_IntrinsicKind kind, RFactory* rFactory)
{
    static QIntrinsicInfo intrinsicInfos[(size_t)QInst_IntrinsicKind::MoveAssign_Void_StringRef_StringMoveRef + 1];
    static bool bInit = [rFactory]() {
        using enum QInst_IntrinsicKind;

        auto* voidType = rFactory->MakeVoidType();
        auto* boolType = rFactory->MakeBoolType();
        auto* intType = rFactory->MakeIntType();
        auto* stringType = rFactory->MakeStringType();

        auto* voidPtrType = rFactory->MakePtrType(voidType);

        RFuncReturn retVoid = RFuncReturn_Set{voidType};
        RFuncReturn retVoidPtr = RFuncReturn_Set{voidPtrType};
        RFuncReturn retBool = RFuncReturn_Set{boolType};
        RFuncReturn retInt = RFuncReturn_Set{intType};
        RFuncReturn retStr = RFuncReturn_Set{stringType};

        auto srp = [stringType](const string& name) -> RFuncParameter { return {RFuncParameterKind::Ref, stringType, RName_Normal{name}}; };
        auto sip = [stringType](const string& name) -> RFuncParameter { return {RFuncParameterKind::In, stringType, RName_Normal{name}}; };
        auto smp = [stringType](const string& name) -> RFuncParameter { return {RFuncParameterKind::Move, stringType, RName_Normal{name}}; };
        auto ip = [intType](const string& name) -> RFuncParameter { return {RFuncParameterKind::Normal, intType, RName_Normal{name}}; };
        auto irp = [intType](const string& name) -> RFuncParameter { return {RFuncParameterKind::Ref, intType, RName_Normal{name}}; };
        auto bp = [boolType](const string& name) -> RFuncParameter { return {RFuncParameterKind::Normal, boolType, RName_Normal{name}}; };
        auto vpp = [voidPtrType](const string& name) -> RFuncParameter { return {RFuncParameterKind::Normal, voidPtrType, RName_Normal{name}}; };
        
        intrinsicInfos[(size_t)Command_Item] = {Command_Item, retVoid, {sip("item")}};
        intrinsicInfos[(size_t)Alloc_Int] = {Alloc_Int, retVoidPtr, {ip("size")}};
        intrinsicInfos[(size_t)Memcpy_Void_Ptr_Ptr_Int] = {Memcpy_Void_Ptr_Ptr_Int, retVoid, {vpp("dest"), vpp("src"), ip("size")}};
        // intrinsicInfos[(size_t)NewList_Items] = {NewList_Items,};
        intrinsicInfos[(size_t)GetIterator_ListPtr_ListIterator] = {GetIterator_ListPtr_ListIterator,};

        intrinsicInfos[(size_t)LogicalNot_Bool_Bool] = {LogicalNot_Bool_Bool, retBool, {bp("x")}};
        intrinsicInfos[(size_t)UnaryMinus_Int_Int] = {UnaryMinus_Int_Int, retInt, {ip("x")}};
        intrinsicInfos[(size_t)ToString_String_Bool] = {ToString_String_Bool, retStr, {bp("x")}};
        intrinsicInfos[(size_t)ToString_String_Int] = {ToString_String_Int, retStr, {ip("x")}};
        intrinsicInfos[(size_t)PrefixInc_Int_IntRef] = {PrefixInc_Int_IntRef, retInt, {irp("x")}};
        intrinsicInfos[(size_t)PrefixDec_Int_IntRef] = {PrefixDec_Int_IntRef, retInt, {irp("x")}};
        intrinsicInfos[(size_t)PostfixInc_Int_IntRef] = {PostfixInc_Int_IntRef, retInt, {irp("x")}};
        intrinsicInfos[(size_t)PostfixDec_Int_IntRef] = {PostfixDec_Int_IntRef, retInt, {irp("x")}};
        intrinsicInfos[(size_t)Multiply_Int_Int_Int] = {Multiply_Int_Int_Int, retInt, {ip("x"), ip("y")}};
        intrinsicInfos[(size_t)Divide_Int_Int_Int] = {Divide_Int_Int_Int, retInt, {ip("x"), ip("y")}};
        intrinsicInfos[(size_t)Modulo_Int_Int_Int] = {Modulo_Int_Int_Int, retInt, {ip("x"), ip("y")}};
        intrinsicInfos[(size_t)Add_Int_Int_Int] = {Add_Int_Int_Int, retInt, {ip("x"), ip("y")}};
        intrinsicInfos[(size_t)Add_String_StringInRef_StringInRef] = {Add_String_StringInRef_StringInRef, retStr, {sip("x"), sip("y")}};
        intrinsicInfos[(size_t)Subtract_Int_Int_Int] = {Subtract_Int_Int_Int, retInt, {ip("x"), ip("y")}};
        intrinsicInfos[(size_t)LessThan_Bool_Int_Int] = {LessThan_Bool_Int_Int, retBool, {ip("x"), ip("y")}};
        intrinsicInfos[(size_t)LessThan_Bool_StringInRef_StringInRef] = {LessThan_Bool_StringInRef_StringInRef, retBool, {sip("x"), sip("y")}};
        intrinsicInfos[(size_t)GreaterThan_Bool_Int_Int] = {GreaterThan_Bool_Int_Int, retBool, {ip("x"), ip("y")}};
        intrinsicInfos[(size_t)GreaterThan_Bool_StringInRef_StringInRef] = {GreaterThan_Bool_StringInRef_StringInRef, retBool, {sip("x"), sip("y")}};
        intrinsicInfos[(size_t)LessThanOrEqual_Bool_Int_Int] = {LessThanOrEqual_Bool_Int_Int, retBool, {ip("x"), ip("y")}};
        intrinsicInfos[(size_t)LessThanOrEqual_Bool_StringInRef_StringInRef] = {LessThanOrEqual_Bool_StringInRef_StringInRef, retBool, {sip("x"), sip("y")}};
        intrinsicInfos[(size_t)GreaterThanOrEqual_Bool_Int_Int] = {GreaterThanOrEqual_Bool_Int_Int, retBool, {ip("x"), ip("y")}};
        intrinsicInfos[(size_t)GreaterThanOrEqual_Bool_StringInRef_StringInRef] = {GreaterThanOrEqual_Bool_StringInRef_StringInRef, retBool, {sip("x"), sip("y")}};
        intrinsicInfos[(size_t)Equal_Bool_Int_Int] = {Equal_Bool_Int_Int, retBool, {ip("x"), ip("y")}};
        intrinsicInfos[(size_t)Equal_Bool_Bool_Bool] = {Equal_Bool_Bool_Bool, retBool, {bp("x"), bp("y")}};
        intrinsicInfos[(size_t)Equal_Bool_StringInRef_StringInRef] = {Equal_Bool_StringInRef_StringInRef, retBool, {sip("x"), sip("y")}};

        intrinsicInfos[(size_t)CopyCtor_Void_StringRef_StringInRef] = {CopyCtor_Void_StringRef_StringInRef, retVoid, {srp("this"), sip("other")}};
        intrinsicInfos[(size_t)MoveCtor_Void_StringRef_StringMoveRef] = {MoveCtor_Void_StringRef_StringMoveRef, retVoid, {srp("this"), smp("other")}};
        intrinsicInfos[(size_t)Dtor_Void_StringRef] = {Dtor_Void_StringRef, retVoid, {srp("this")}};
        intrinsicInfos[(size_t)CopyAssign_Void_StringRef_StringInRef] = {CopyAssign_Void_StringRef_StringInRef, retVoid, {srp("this"), sip("other")}};
        intrinsicInfos[(size_t)MoveAssign_Void_StringRef_StringMoveRef] = {MoveAssign_Void_StringRef_StringMoveRef, retVoid, {srp("this"), smp("other")}};
        return true;
    }();

    assert(bInit);
    return &intrinsicInfos[(size_t)kind];
}

QIntrinsicInfo* GetIntrinsicInfo(MExp_CallIntrinsicKind kind, RFactory* rFactory)
{
    static unordered_map<MExp_CallIntrinsicKind, QIntrinsicInfo*> m{
        {MExp_CallIntrinsicKind::LogicalNot_Bool_Bool, GetIntrinsicInfo(QInst_IntrinsicKind::LogicalNot_Bool_Bool, rFactory)},
        {MExp_CallIntrinsicKind::UnaryMinus_Int_Int, GetIntrinsicInfo(QInst_IntrinsicKind::UnaryMinus_Int_Int, rFactory)},
        {MExp_CallIntrinsicKind::PrefixInc_Int_IntRef, GetIntrinsicInfo(QInst_IntrinsicKind::PrefixInc_Int_IntRef, rFactory)},
        {MExp_CallIntrinsicKind::PrefixDec_Int_IntRef, GetIntrinsicInfo(QInst_IntrinsicKind::PrefixDec_Int_IntRef, rFactory)},
        {MExp_CallIntrinsicKind::PostfixInc_Int_IntRef, GetIntrinsicInfo(QInst_IntrinsicKind::PostfixInc_Int_IntRef, rFactory)},
        {MExp_CallIntrinsicKind::PostfixDec_Int_IntRef, GetIntrinsicInfo(QInst_IntrinsicKind::PostfixDec_Int_IntRef, rFactory)},
        {MExp_CallIntrinsicKind::Multiply_Int_Int_Int, GetIntrinsicInfo(QInst_IntrinsicKind::Multiply_Int_Int_Int, rFactory)},
        {MExp_CallIntrinsicKind::Divide_Int_Int_Int, GetIntrinsicInfo(QInst_IntrinsicKind::Divide_Int_Int_Int, rFactory)},
        {MExp_CallIntrinsicKind::Modulo_Int_Int_Int, GetIntrinsicInfo(QInst_IntrinsicKind::Modulo_Int_Int_Int, rFactory)},
        {MExp_CallIntrinsicKind::Add_Int_Int_Int, GetIntrinsicInfo(QInst_IntrinsicKind::Add_Int_Int_Int, rFactory)},
        {MExp_CallIntrinsicKind::Subtract_Int_Int_Int, GetIntrinsicInfo(QInst_IntrinsicKind::Subtract_Int_Int_Int, rFactory)},
        {MExp_CallIntrinsicKind::LessThan_Bool_Int_Int, GetIntrinsicInfo(QInst_IntrinsicKind::LessThan_Bool_Int_Int, rFactory)},
        {MExp_CallIntrinsicKind::LessThan_Bool_StringInRef_StringInRef, GetIntrinsicInfo(QInst_IntrinsicKind::LessThan_Bool_StringInRef_StringInRef, rFactory)},
        {MExp_CallIntrinsicKind::GreaterThan_Bool_Int_Int, GetIntrinsicInfo(QInst_IntrinsicKind::GreaterThan_Bool_Int_Int, rFactory)},
        {MExp_CallIntrinsicKind::GreaterThan_Bool_StringInRef_StringInRef, GetIntrinsicInfo(QInst_IntrinsicKind::GreaterThan_Bool_StringInRef_StringInRef, rFactory)},
        {MExp_CallIntrinsicKind::LessThanOrEqual_Bool_Int_Int, GetIntrinsicInfo(QInst_IntrinsicKind::LessThanOrEqual_Bool_Int_Int, rFactory)},
        {MExp_CallIntrinsicKind::LessThanOrEqual_Bool_StringInRef_StringInRef, GetIntrinsicInfo(QInst_IntrinsicKind::LessThanOrEqual_Bool_StringInRef_StringInRef, rFactory)},
        {MExp_CallIntrinsicKind::GreaterThanOrEqual_Bool_Int_Int, GetIntrinsicInfo(QInst_IntrinsicKind::GreaterThanOrEqual_Bool_Int_Int, rFactory)},
        {MExp_CallIntrinsicKind::GreaterThanOrEqual_Bool_StringInRef_StringInRef, GetIntrinsicInfo(QInst_IntrinsicKind::GreaterThanOrEqual_Bool_StringInRef_StringInRef, rFactory)},
        {MExp_CallIntrinsicKind::Equal_Bool_Int_Int, GetIntrinsicInfo(QInst_IntrinsicKind::Equal_Bool_Int_Int, rFactory)},
        {MExp_CallIntrinsicKind::Equal_Bool_Bool_Bool, GetIntrinsicInfo(QInst_IntrinsicKind::Equal_Bool_Bool_Bool, rFactory)},
        {MExp_CallIntrinsicKind::Equal_Bool_StringInRef_StringInRef, GetIntrinsicInfo(QInst_IntrinsicKind::Equal_Bool_StringInRef_StringInRef, rFactory)},
        {MExp_CallIntrinsicKind::GetIterator_ListPtr_ListIterator, GetIntrinsicInfo(QInst_IntrinsicKind::GetIterator_ListPtr_ListIterator, rFactory)},
    };

    auto i = m.find(kind);
    if (i == m.end()) return nullptr;

    return i->second;
}

QIntrinsicInfo* GetIntrinsicInfo(MInitExp_CallIntrinsicKind kind, RFactory* rFactory)
{
    static unordered_map<MInitExp_CallIntrinsicKind, QIntrinsicInfo*> m = {
        {MInitExp_CallIntrinsicKind::ToString_String_Bool, GetIntrinsicInfo(QInst_IntrinsicKind::ToString_String_Bool, rFactory)},
        {MInitExp_CallIntrinsicKind::ToString_String_Int, GetIntrinsicInfo(QInst_IntrinsicKind::ToString_String_Int, rFactory)},
        {MInitExp_CallIntrinsicKind::Add_String_StringInRef_StringInRef, GetIntrinsicInfo(QInst_IntrinsicKind::Add_String_StringInRef_StringInRef, rFactory)},
    };

    auto i = m.find(kind);
    if (i == m.end()) return nullptr;

    return i->second;
}

} // namespace Citron