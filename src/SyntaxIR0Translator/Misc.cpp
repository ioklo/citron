#include "pch.h"
#include "Misc.h"

#include <Infra/Ptr.h>
#include <Infra/NotImplementedException.h>
#include <Logging/Logger.h>
#include <IR0/RType.h>
#include <IR0/RExp.h>

#include "ScopeContext.h"

namespace Citron::SyntaxIR0Translator {

RExpPtr TryCastRExp(const RExpPtr& exp, const RTypePtr& expectedType, const ScopeContextPtr& context) // nothrow
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
RExpPtr CastRExp(const RExpPtr& exp, const RTypePtr& expectedType, const ScopeContextPtr& context, const LoggerPtr& logger)
{
    auto result = TryCastRExp(exp, expectedType, context);
    if (result != nullptr) return result;

    logger->Fatal_CastFailed();
    return nullptr;
}

RExpPtr MakeRAsExp(const RTypePtr& targetType, const RTypePtr& testType, RExpPtr&& targetExp)
{
    auto targetTypeKind = targetType->GetCustomTypeKind();
    auto testTypeKind = testType->GetCustomTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RCustomTypeKind::Class)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<RClassAsClassExp>(std::move(targetExp), testType);

        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<RInterfaceAsClassExp>(std::move(targetExp), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::Interface)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<RClassAsInterfaceExp>(std::move(targetExp), testType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<RInterfaceAsInterfaceExp>(std::move(targetExp), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::EnumElem)
    {
        if (targetTypeKind == RCustomTypeKind::Enum)
            return MakePtr<REnumAsEnumElemExp>(std::move(targetExp), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else
        throw NotImplementedException(); // 에러 처리
}

} // namespace Citron::SyntaxIR0Translator