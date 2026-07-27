#include "Misc.h"

#include <cassert>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Logging/Logger.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RFactory.h"
#include "MIR/MExp.h"
#include "MIR/MFactory.h"
#include "RSymbol/RTypes.h"

#include "SmScopeContext.h"
#include "SmTranslationContexts.h"

using namespace std;

namespace Citron {

class RTypeArguments;

expected<RFuncParameterKind, DiagPtr> MakeParamKind(optional<SParamModifier> o_modifier, bool bRef)
{   
    if (!o_modifier)
    {
        if (bRef) return RFuncParameterKind::Ref;
        return RFuncParameterKind::Normal;
    }

    switch (*o_modifier)
    {
    case SParamModifier::In: 
        if (!bRef) return unexpected{MakePtr<Error_FuncDecl_ParameterKindNeedRef>()};
        return RFuncParameterKind::In;

    case SParamModifier::Move: 
        if (!bRef) return unexpected{MakePtr<Error_FuncDecl_ParameterKindNeedRef>()};
        return RFuncParameterKind::Move;

    case SParamModifier::Forward:
        if (!bRef) return unexpected{MakePtr<Error_FuncDecl_ParameterKindNeedRef>()};
        return RFuncParameterKind::Forward;

    case SParamModifier::Out: 
        if (!bRef) return unexpected{MakePtr<Error_FuncDecl_ParameterKindNeedRef>()};
        return RFuncParameterKind::Out;

    case SParamModifier::Params: 
        return RFuncParameterKind::Params;
        
    default: unreachable();
    }
}

expected<RTypeArguments*, DiagPtr> MakeRTypeArgs(std::vector<STypeExp*>& typeArgs, SmTranslationContexts& contexts)
{
    std::vector<RType*> items;
    items.reserve(typeArgs.size());

    for (auto* typeArg : typeArgs)
    {
        auto e_type = contexts.scopeContext->TranslateSTypeExpToRType(typeArg);
        RETURN_ON_ERROR(e_type);

        items.push_back(*e_type);
    }

    return contexts.rFactory->MakeTypeArguments(items);
}

// TODO: implementation을 CastNExp로 옮긴다
//MExp* TryCastRExp(MExp* exp, RType* expectedType, SmTranslationContexts& contexts) // nothrow
//{
//    static_assert(false);
//
//    //auto expType = exp->GetType();
//    //auto expTypeKind = expType->GetTypeKind();
//
//    //auto expectedTypeKind = expectedType->GetTypeKind();
//
//    //// 같으면 그대로 리턴
//    //if (expectedType == expType)
//    //    return exp;
//
//    //// 1. enumElem -> enum
//    //if (expTypeKind == RCustomTypeKind::EnumElem)
//    //{
//    //    if (expectedTypeKind == RCustomTypeKind::Enum)
//    //    {
//    //        if (expectedType == expType TypeEquals(expectedEnumType, enumElemType.GetEnumType()))
//    //        {
//    //            return new R.CastEnumElemToEnumExp(exp, expectedEnumType.GetSymbol());
//    //        }
//    //    }
//
//    //    return null;
//    //}
//
//    //// 2. exp is class type
//    //if (expType is ClassSymbol @class)
//    //{
//    //    if (expectedType is ClassSymbol expectedClass)
//    //    {
//    //        // allows upcast
//    //        if (expectedClass.IsBaseOf(@class))
//    //        {
//    //            return new R.CastClassExp(exp, expectedClass);
//    //        }
//
//    //        return null;
//    //    }
//
//    //    // TODO: interface
//    //    // if (expectType is InterfaceTypeValue )
//    //}
//
//    // TODO: 3. C -> Nullable<C>, C -> B -> Nullable<B> 허용
//    //if (IsNullableType(expectedType, out var expectedInnerType))
//    //{
//    //    // C -> B 시도
//    //    var castToInnerTypeExp = TryCastExp_Exp(exp, expectedInnerType);
//    //    if (castToInnerTypeExp != null)
//    //    {
//    //        // B -> B?
//    //        return MakeNullableExp(castToInnerTypeExp);
//    //        return new R.NewNullableExp(castToInnerTypeExp, expectedNullableType);
//    //    }
//    //}
//
//    return nullptr;
//}

// 값의 겉보기 타입을 변경한다
// TODO: [47] CastMExp의 리턴값 수정, NBC의 암시적 Cast구현하기
//expected<MExp*, DiagPtr> CastMExp(MExp* exp, RType* expectedType, SmTranslationContexts& contexts)
//{
//    auto* expType = exp->GetType();
//
//    // 같으면 그대로 리턴
//    if (expectedType == expType)
//        return exp;
//
//    // 1. enumElem인 경우, enum으로 변경할 수 있다
//    if (auto* expEnumElemType = dynamic_cast<RType_EnumElem*>(expType))
//    {
//        auto expEnumType = expEnumElemType->GetBaseEnumType();
//
//        if (expectedType == expEnumType)
//            return contexts.mFactory->MakeMExp<MExp_CastEnumElemToEnum>(exp, expectedType);
//
//        // 에러가 좀더 구체적으로 알려줬으면 좋겠다
//        throw NotImplementedException{};
//        return unexpected{MakePtr<Error_Cast_Failed>()};
//    }
//
//    // 2. exp is class type
//    if (auto* expClassType = dynamic_cast<RType_Class*>(expType))
//    {
//        if (auto* expectedClassType = dynamic_cast<RType_Class*>(expectedType))
//        {
//            if (expectedClassType->IsBaseOf(*expClassType))
//            {
//                return contexts.mFactory->MakeMExp<MExp_CastClass>(exp, expectedClassType);
//            }
//        }
//
//        return unexpected{MakePtr<Error_Cast_Failed>()};
//        // TODO: interface
//        // if (expectType is InterfaceTypeValue )
//    }
//
//    // TODO: 3. C -> Nullable<C>, C -> B -> Nullable<B> 허용
//    if (auto* expectedNullableType = dynamic_cast<RType_NullableInplace*>(expectedType))
//    {
//        // Nullable<B>를 원한다면 C를 B로 변환해본다
//        auto e_castToInnerTypeExp = CastMExp(exp, expectedNullableType->innerType, contexts);
//        if (!e_castToInnerTypeExp)
//            return unexpected{MakePtr<Error_Cast_Failed>()};
//
//        // B?로 변경
//        return contexts.mFactory->MakeMExp<MExp_Nullable>(*e_castToInnerTypeExp, contexts.rFactory);
//    }
//
//    return unexpected{MakePtr<Error_Cast_Failed>()};
//}

bool IsVarType(STypeExp* typeExp)
{
    auto* idTypeExp = dynamic_cast<STypeExp_Id*>(typeExp);
    return idTypeExp && idTypeExp->name == "var" && idTypeExp->typeArgs.size() == 0;
}

} // namespace Citron