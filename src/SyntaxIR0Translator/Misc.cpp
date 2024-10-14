#include "pch.h"
#include "Misc.h"

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Logging/Logger.h>

#include <IR0/RTypeFactory.h>
#include <IR0/RType.h>
#include <IR0/RExp.h>

#include "ScopeContext.h"


namespace Citron::SyntaxIR0Translator {

RTypeArgumentsPtr MakeTypeArgs(std::vector<STypeExpPtr>& typeArgs, ScopeContext& context, RTypeFactory& factory)
{
    std::vector<RTypePtr> items;
    items.reserve(typeArgs.size());

    for (auto& typeArg : typeArgs)
    {
        auto type = context.MakeType(*typeArg, factory);
        if (!type) return nullptr;

        items.push_back(std::move(type));
    }

    return factory.MakeTypeArguments(items);
}


RExpPtr TryCastRExp(RExpPtr&& exp, const RTypePtr& expectedType, ScopeContext& context) // nothrow
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
RExpPtr CastRExp(RExpPtr&& exp, const RTypePtr& expectedType, ScopeContext& context, Logger& logger)
{
    auto result = TryCastRExp(std::move(exp), expectedType, context);
    if (result != nullptr) return result;

    logger.Fatal_Cast_Failed();
    return nullptr;
}

RExpPtr MakeRExp_As(RExpPtr&& targetExp, const RTypePtr& testType, RTypeFactory& factory)
{
    auto targetType = targetExp->GetType(factory);
    auto targetTypeKind = targetType->GetCustomTypeKind();
    auto testTypeKind = testType->GetCustomTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RCustomTypeKind::Class)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<RExp_ClassAsClass>(std::move(targetExp), testType);

        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<RExp_InterfaceAsClass>(std::move(targetExp), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::Interface)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<RExp_ClassAsInterface>(std::move(targetExp), testType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<RExp_InterfaceAsInterface>(std::move(targetExp), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::EnumElem)
    {
        if (targetTypeKind == RCustomTypeKind::Enum)
            return MakePtr<RExp_EnumAsEnumElem>(std::move(targetExp), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else
        throw NotImplementedException(); // 에러 처리
}


} // namespace Citron::SyntaxIR0Translator