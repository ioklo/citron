#include "pch.h"
#include "Misc.h"

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Logging/Logger.h>

#include <Syntax/Syntax.h>

#include <IR0/RTypeFactory.h>
#include <IR0/RType.h>
#include <IR0/NExp.h>

#include "ScopeContext.h"
#include "TranslationContext.h"

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


NExpPtr TryCastRExp(NExpPtr&& exp, const RTypePtr& expectedType, TranslationContext& context) // nothrow
{
    static_assert(false);

    //auto expType = exp->GetType();
    //auto expTypeKind = expType->GetCustomTypeKind();

    //auto expectedTypeKind = expectedType->GetCustomTypeKind();

    //// 같으면 그대로 리턴
    //if (expectedType == expType)
    //    return exp;

    //// 1. enumElem -> enum
    //if (expTypeKind == RCustomTypeKind::EnumElem)
    //{
    //    if (expectedTypeKind == RCustomTypeKind::Enum)
    //    {
    //        if (expectedType == expType TypeEquals(expectedEnumType, enumElemType.GetEnumType()))
    //        {
    //            return new R.CastEnumElemToEnumExp(exp, expectedEnumType.GetSymbol());
    //        }
    //    }

    //    return null;
    //}

    //// 2. exp is class type
    //if (expType is ClassSymbol @class)
    //{
    //    if (expectedType is ClassSymbol expectedClass)
    //    {
    //        // allows upcast
    //        if (expectedClass.IsBaseOf(@class))
    //        {
    //            return new R.CastClassExp(exp, expectedClass);
    //        }

    //        return null;
    //    }

    //    // TODO: interface
    //    // if (expectType is InterfaceTypeValue )
    //}

    // TODO: 3. C -> Nullable<C>, C -> B -> Nullable<B> 허용
    //if (IsNullableType(expectedType, out var expectedInnerType))
    //{
    //    // C -> B 시도
    //    var castToInnerTypeExp = TryCastExp_Exp(exp, expectedInnerType);
    //    if (castToInnerTypeExp != null)
    //    {
    //        // B -> B?
    //        return MakeNullableExp(castToInnerTypeExp);
    //        return new R.NewNullableExp(castToInnerTypeExp, expectedNullableType);
    //    }
    //}

    return nullptr;
}


// 값의 겉보기 타입을 변경한다
NExpPtr CastNExp(NExpPtr&& exp, const RTypePtr& expectedType, TranslationContext& context)
{
    auto result = TryCastRExp(std::move(exp), expectedType, context);
    if (result != nullptr) return result;

    context.Log(&Logger::Fatal_Cast_Failed);
    return nullptr;
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
        throw new RuntimeFatalException();
    }
}

} // namespace Citron::SyntaxIR0Translator