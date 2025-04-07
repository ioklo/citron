module Citron.SyntaxIR0Translator:Misc;

import <cassert>;

import Citron.Ptr;
import Citron.Exceptions;
import Citron.Logger;

import Citron.Syntax;

import Citron.RDecls;
import Citron.NDecls;

import :ScopeContext;
import :TranslationContext;

namespace Citron::SyntaxIR0Translator {

RTypeArgumentsPtr MakeTypeArgs(std::vector<STypeExpPtr>& typeArgs, TranslationContext& context)
{
    std::vector<RTypePtr> items;
    items.reserve(typeArgs.size());

    for (auto& typeArg : typeArgs)
    {
        auto type = context.TranslateSTypeExpToRType(*typeArg);
        if (!type) return nullptr;

        items.push_back(std::move(type));
    }

    return context.MakeTypeArguments(items);
}

// TODO: implementation을 CastNExp로 옮긴다
//NExpPtr TryCastRExp(NExpPtr&& exp, const RTypePtr& expectedType, TranslationContext& context) // nothrow
//{
//    static_assert(false);
//
//    //auto expType = exp->GetType();
//    //auto expTypeKind = expType->GetCustomTypeKind();
//
//    //auto expectedTypeKind = expectedType->GetCustomTypeKind();
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
expected<NExpPtr, DiagPtr> CastNExp(NExpPtr&& exp, const RTypePtr& expectedType, TranslationContext& context)
{
    // 위 구현을 참고하여 작성한다
    static_assert(false);

    /*auto result = TryCastRExp(std::move(exp), expectedType, context);
    if (result != nullptr) return result;

    return unexpected{MakePtr<Error_Cast_Failed>()};*/
}

export std<Citron::NExpPtr, Citron::DiagPtr> CastNExp(const NExpPtr& exp, const RTypePtr& expectedType, TranslationContext& context)
{
    static_assert(false);
}

bool IsVarType(STypeExp& typeExp)
{   
    auto* idTypeExp = dynamic_cast<STypeExp_Id*>(&typeExp);
    return idTypeExp && idTypeExp->name == "var" && idTypeExp->typeArgs.size() == 0;
}

RName_CtorParam MakeBaseCtorParamName(size_t index, RName baseParamName)
{
    if (auto* specialName = get_if<RName_CtorParam>(&baseParamName))
    {
        return RName_CtorParam(index, specialName->paramText);
    }
    else if (auto* normalName = get_if<RName_Normal>(&baseParamName))
    {
        return RName_CtorParam(index, normalName->text);
    }
    else
    {
        throw RuntimeFatalException();
    }
}

} // namespace Citron::SyntaxIR0Translator