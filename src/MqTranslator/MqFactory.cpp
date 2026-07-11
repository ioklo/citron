#include "MqFactory.h"
#include <string>
#include "RSymbol/RFactory.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFuncParameter.h"

using namespace std;

namespace Citron {

MqFactory::MqFactory(TakeRef<RFactoryPtr> rFactory)
    : rFactory{rFactory.Take()}
{
    MakeIntrinsicInfo();
}

void MqFactory::MakeIntrinsicInfo() noexcept
{
    using enum QInst_IntrinsicKind;

    auto* voidType = rFactory->MakeVoidType();
    auto* boolType = rFactory->MakeBoolType();
    auto* intType = rFactory->MakeIntType();
    auto* stringType = rFactory->MakeStringType();

    auto* voidPtrType = rFactory->MakePtrType(voidType);

    RFuncReturn retVoid = RFuncReturn_Normal{voidType};
    RFuncReturn retVoidPtr = RFuncReturn_Normal{voidPtrType};
    RFuncReturn retBool = RFuncReturn_Normal{boolType};
    RFuncReturn retInt = RFuncReturn_Normal{intType};
    RFuncReturn retStr = RFuncReturn_Normal{stringType};

    auto srp = [stringType](const string& name) -> RFuncParameter { return {RFuncParameterKind::Ref, stringType, RName::Normal(name)}; };
    auto sip = [stringType](const string& name) -> RFuncParameter { return {RFuncParameterKind::In, stringType, RName::Normal(name)}; };
    auto smp = [stringType](const string& name) -> RFuncParameter { return {RFuncParameterKind::Move, stringType, RName::Normal(name)}; };
    auto ip = [intType](const string& name) -> RFuncParameter { return {RFuncParameterKind::Normal, intType, RName::Normal(name)}; };
    auto irp = [intType](const string& name) -> RFuncParameter { return {RFuncParameterKind::Ref, intType, RName::Normal(name)}; };
    auto bp = [boolType](const string& name) -> RFuncParameter { return {RFuncParameterKind::Normal, boolType, RName::Normal(name)}; };
    auto vpp = [voidPtrType](const string& name) -> RFuncParameter { return {RFuncParameterKind::Normal, voidPtrType, RName::Normal(name)}; };

    intrinsicInfos[(size_t)Command_Item] = {Command_Item, retVoid, {sip("item")}};
    intrinsicInfos[(size_t)Alloc_Int] = {Alloc_Int, retVoidPtr, {ip("size")}};
    intrinsicInfos[(size_t)Memcpy_Void_Ptr_Ptr_Int] = {Memcpy_Void_Ptr_Ptr_Int, retVoid, {vpp("dest"), vpp("src"), ip("size")}};
    // intrinsicInfos[(size_t)NewList_Items] = {NewList_Items,};
    // intrinsicInfos[(size_t)GetIterator_ListPtr_ListIterator] = {GetIterator_ListPtr_ListIterator, , {}};

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

    mExpToMq[(size_t)MExp_CallIntrinsicKind::LogicalNot_Bool_Bool] = &intrinsicInfos[(size_t)LogicalNot_Bool_Bool];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::UnaryMinus_Int_Int] = &intrinsicInfos[(size_t)UnaryMinus_Int_Int];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::PrefixInc_Int_IntRef] = &intrinsicInfos[(size_t)PrefixInc_Int_IntRef];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::PrefixDec_Int_IntRef] = &intrinsicInfos[(size_t)PrefixDec_Int_IntRef];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::PostfixInc_Int_IntRef] = &intrinsicInfos[(size_t)PostfixInc_Int_IntRef];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::PostfixDec_Int_IntRef] = &intrinsicInfos[(size_t)PostfixDec_Int_IntRef];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::Multiply_Int_Int_Int] = &intrinsicInfos[(size_t)Multiply_Int_Int_Int];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::Divide_Int_Int_Int] = &intrinsicInfos[(size_t)Divide_Int_Int_Int];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::Modulo_Int_Int_Int] = &intrinsicInfos[(size_t)Modulo_Int_Int_Int];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::Add_Int_Int_Int] = &intrinsicInfos[(size_t)Add_Int_Int_Int];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::Subtract_Int_Int_Int] = &intrinsicInfos[(size_t)Subtract_Int_Int_Int];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::LessThan_Bool_Int_Int] = &intrinsicInfos[(size_t)LessThan_Bool_Int_Int];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::LessThan_Bool_StringInRef_StringInRef] = &intrinsicInfos[(size_t)LessThan_Bool_StringInRef_StringInRef];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::GreaterThan_Bool_Int_Int] = &intrinsicInfos[(size_t)GreaterThan_Bool_Int_Int];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::GreaterThan_Bool_StringInRef_StringInRef] = &intrinsicInfos[(size_t)GreaterThan_Bool_StringInRef_StringInRef];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::LessThanOrEqual_Bool_Int_Int] = &intrinsicInfos[(size_t)LessThanOrEqual_Bool_Int_Int];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::LessThanOrEqual_Bool_StringInRef_StringInRef] = &intrinsicInfos[(size_t)LessThanOrEqual_Bool_StringInRef_StringInRef];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::GreaterThanOrEqual_Bool_Int_Int] = &intrinsicInfos[(size_t)GreaterThanOrEqual_Bool_Int_Int];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::GreaterThanOrEqual_Bool_StringInRef_StringInRef] = &intrinsicInfos[(size_t)GreaterThanOrEqual_Bool_StringInRef_StringInRef];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::Equal_Bool_Int_Int] = &intrinsicInfos[(size_t)Equal_Bool_Int_Int];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::Equal_Bool_Bool_Bool] = &intrinsicInfos[(size_t)Equal_Bool_Bool_Bool];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::Equal_Bool_StringInRef_StringInRef] = &intrinsicInfos[(size_t)Equal_Bool_StringInRef_StringInRef];
    mExpToMq[(size_t)MExp_CallIntrinsicKind::GetIterator_ListPtr_ListIterator] = &intrinsicInfos[(size_t)GetIterator_ListPtr_ListIterator];

    mInitExpToMq[(size_t)MInitExp_CallIntrinsicKind::ToString_String_Bool] = &intrinsicInfos[(size_t)ToString_String_Bool];
    mInitExpToMq[(size_t)MInitExp_CallIntrinsicKind::ToString_String_Int] = &intrinsicInfos[(size_t)ToString_String_Int];
    mInitExpToMq[(size_t)MInitExp_CallIntrinsicKind::Add_String_StringInRef_StringInRef] = &intrinsicInfos[(size_t)Add_String_StringInRef_StringInRef];
}

} // namespace Citron