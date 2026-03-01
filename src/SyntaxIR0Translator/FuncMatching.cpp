#include "FuncMatching.h"
#include <expected>
#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"
#include "Logging/Diag.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RFuncDecl.h"
#include "RSymbol/RTypeParamDecl.h"
#include "TranslationContexts.h"
#include "SExpToMOperandTranslation.h"
#include "Misc.h"

using namespace std;

namespace Citron {

size_t RFuncDeclMatchArgumentsInput::GetTypeParamCount()
{
    return funcDecl->GetTypeParamCount();
}

RTypeParamDecl* RFuncDeclMatchArgumentsInput::GetTypeParam(size_t index)
{
    return funcDecl->GetTypeParam(index);
}

size_t RFuncDeclMatchArgumentsInput::GetFuncParamCount()
{
    return funcDecl->GetUnboundFuncParams().size();
}

RFuncParameter RFuncDeclMatchArgumentsInput::GetFuncParam(RTypeArguments* typeArgs, size_t index)
{
    return funcDecl->GetFuncParam(*typeArgs, index);
}

RTypeArguments* MakeTypeArgs(IMatchArgumentsInput* input, RTypeArguments* outerTypeArgs, RTypeArguments* partialTypeArgsExceptOuter, RFactory& rFactory)
{
    size_t partialArgCount = partialTypeArgsExceptOuter->GetCount();
    size_t paramCount = input->GetTypeParamCount();

    // partial typeArgs를 open으로 꽉 채운다
    // [T1, T2, int, T4]
    // 먼저 [int, T4]부터 만든다
    assert(partialArgCount <= paramCount);
    std::vector<RType*> argsItems;
    argsItems.reserve(paramCount);
    for (size_t i = 0; i < partialArgCount; ++i)
        argsItems.push_back(partialTypeArgsExceptOuter->Get(i));
    for (size_t i = partialArgCount; i < paramCount; ++i)
        argsItems.push_back(rFactory.MakeTypeVarType(input->GetTypeParam(i)));
    auto* args = rFactory.MakeTypeArguments(std::move(argsItems));

    // 이제 outer typeArgs와 합친다
    return rFactory.MergeTypeArguments(*outerTypeArgs, *args);
}

struct TypeEqualConstraint
{
    RType* x;
    RType* y;
};

bool IsOpenType(RType* type)
{
    throw NotImplementedException{};
}

expected<void, DiagPtr> CheckType(vector<TypeEqualConstraint>& constraints, RType* actual, RType* expected)
{
    if (actual == expected) return {};

    // 타입이 1) rFuncParam이 openType이라 constraint에 넣어야 하는 경우 2) 실제로 맞지 않는 경우
    if (IsOpenType(expected))
    {
        constraints.push_back(TypeEqualConstraint{expected, actual});
        return {};
    }

    return unexpected{MakePtr<Error_FuncMatch_MismatchBetweenParamTypeAndArgType>()};
}

expected<optional<ArgumentsMatch>, DiagPtr> MatchArguments(
    IMatchArgumentsInput* input,
    RTypeArguments* outerTypeArgs, 
    RTypeArguments* partialTypeArgsExceptOuter,
    SArguments* sArgs,
    TranslationContexts& contexts)
{
    auto* typeArgs = MakeTypeArgs(input, outerTypeArgs, partialTypeArgsExceptOuter, *contexts.rFactory);

    std::vector<TypeEqualConstraint> constraints;

    // TODO: 가변인자 처리    
    auto funcParamCount = input->GetFuncParamCount();
    auto argCount = sArgs->items.size();
    if (funcParamCount != argCount)
        return unexpected{MakePtr<Error_FuncMatch_MismatchBetweenParamCountAndArgCount>()};

    std::vector<MArgument> mArgs;
    mArgs.reserve(argCount); // TODO: 메모리 재사용 최적화
    for (size_t i = 0, count = argCount; i < count; ++i)
    {
        auto* sArgItem = sArgs->items[i];
        auto rFuncParam = input->GetFuncParam(typeArgs, i);
        
        // TODO: [in], [move], [out] 별로 다르게 적용
        if (rFuncParam.kind == RFuncParameterKind::In) // lvalue, rvalue, talias
        {
            // SArgumentModifier 매칭
            if (sArgItem->o_modifier)
            {
                // F(ref s)
                if (*sArgItem->o_modifier == SArgModifier::Ref) // optional modifier, lvalue만 허용
                {
                    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag{};
                    auto e_mLoc = TranslateSExpToMLoc(sArgItem->exp, rFuncParam.type, /*bMaterializeExp*/false, &designatedDiag, contexts);
                    RETURN_ON_ERROR(e_mLoc);

                    auto e_result = CheckType(constraints, (*e_mLoc)->GetType(), rFuncParam.type);
                    RETURN_ON_ERROR(e_result);

                    mArgs.push_back(MArgument_Loc{*e_mLoc});
                }
                else // 나머지는 다 에러
                {
                    // TODO: [29] SArgModifier맞지 않았을때 Error 내도록
                    throw NotImplementedException{};
                }
            }
            else
            {


                DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag{};
                auto e_mLoc = TranslateSExpToMLoc(sArgItem->exp, rFuncParam.type, /*bMaterializeExp*/true, &designatedDiag, contexts);
                RETURN_ON_ERROR(e_mLoc);

                auto e_result = CheckType(constraints, (*e_mLoc)->GetType(), rFuncParam.type);
                RETURN_ON_ERROR(e_result);

                mArgs.push_back(MArgument_Ref{*e_mLoc});
            }
        }
        else if (rFuncParam.kind == RFuncParameterKind::Init)
        {   
            // 특수 파라미터, 함수가 매칭이 되면, 멤버 초기화 구문으로 바뀐다
            // x = expr;
            // expr이 lvalue라면 a 
            // expr이 rvalue라면 F()
            // expr이 move로 시작했다면, move a

            // modifier가 없으면, 평소대로 시도
            if (!sArgItem->o_modifier)
            {
                auto e_mOperand = TranslateSExpToMOperand(sArgItem->exp, rFuncParam.type, contexts);
                RETURN_ON_ERROR(e_mOperand);

                auto e_mArg = visit([&constraints, type = rFuncParam.type](auto& mOperand) -> expected<MArgument, DiagPtr> {
                    using T = remove_cvref_t<decltype(mOperand)>;
                    if constexpr (same_as<T, MOperand_Loc>)
                    {
                        auto e_result = CheckType(constraints, mOperand.loc->GetType(), type);
                        RETURN_ON_ERROR(e_result);

                        return MArgument_Ref{mOperand.loc};
                    }
                    else if constexpr (same_as<T, MOperand_Exp>)
                    {
                        auto e_result = CheckType(constraints, mOperand.exp->GetType(), type);
                        RETURN_ON_ERROR(e_result);

                        return MArgument_Exp{mOperand.exp};
                    }
                    else static_assert(false);
                }, *e_mOperand);
                RETURN_ON_ERROR(e_mArg);

                mArgs.push_back(*e_mArg);
            }
            else if (*sArgItem->o_modifier == SArgModifier::Move)
            {
                // move expr
                DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag{};
                auto e_mLoc = TranslateSExpToMLoc(sArgItem->exp, rFuncParam.type, /*bMaterializeExp*/false, &designatedDiag, contexts);
                RETURN_ON_ERROR(e_mLoc);

                auto e_result = CheckType(constraints, (*e_mLoc)->GetType(), rFuncParam.type);
                RETURN_ON_ERROR(e_result);

                mArgs.push_back(MArgument_Move{*e_mLoc});
            }
            else
            {
                // TODO: 에러
                throw NotImplementedException{};
            }
        }
        else if (rFuncParam.kind == RFuncParameterKind::Ref)
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag{};
            auto e_mLoc = TranslateSExpToMLoc(sArgItem->exp, rFuncParam.type, /*bMaterializeExp*/false, &designatedDiag, contexts);
            RETURN_ON_ERROR(e_mLoc);

            auto* locType = (*e_mLoc)->GetType();
            if (locType != rFuncParam.type)
            {
                // 타입이 1) rFuncParam이 openType이라 constraint에 넣어야 하는 경우 2) 실제로 맞지 않는 경우
                if (IsOpenType(rFuncParam.type))
                    constraints.emplace_back(rFuncParam.type, locType);
                else
                    return unexpected{MakePtr<Error_FuncMatch_MismatchBetweenParamTypeAndArgType>()};
            }

            mArgs.push_back(MArgument_Ref{*e_mLoc});
        }
        else if (rFuncParam.kind == RFuncParameterKind::Normal)
        {
            auto e_mExp = TranslateSExpToMExp(sArgItem->exp, /*hintType*/rFuncParam.type, contexts);
            RETURN_ON_ERROR(e_mExp);
            
            auto* expType = (*e_mExp)->GetType();
            if (expType != rFuncParam.type)
            {
                // 타입이 1) rFuncParam이 openType이라 constraint에 넣어야 하는 경우 2) 실제로 맞지 않는 경우
                if (IsOpenType(rFuncParam.type))
                {
                    constraints.emplace_back(rFuncParam.type, expType);
                    mArgs.push_back(MArgument_Exp{*e_mExp});
                }
                else
                {
                    // 캐스팅 시도
                    auto e_castMExp = CastMExp(*e_mExp, rFuncParam.type, contexts);

                    if (dynamic_pointer_cast<Error_Cast_Failed>(e_castMExp.error()))
                        return unexpected{MakePtr<Error_FuncMatch_MismatchBetweenParamTypeAndArgType>()};

                    RETURN_ON_ERROR(e_castMExp);
                    mArgs.push_back(MArgument_Exp{*e_castMExp});
                }
            }
            else
            {
                mArgs.push_back(MArgument_Exp{*e_mExp});
            }
        }
        else assert(false);
    }

    // TODO: contraint resolver, 일단은 넘어간다
    assert(constraints.empty());

    return ArgumentsMatch{typeArgs, std::move(mArgs)};
}


} // namespace Citron