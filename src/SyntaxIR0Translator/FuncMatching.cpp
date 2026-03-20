#include "FuncMatching.h"
#include <expected>
#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"
#include "Logging/Diag.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"
#include "MIR/MInitExp.h"
#include "MIR/MFactory.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RFuncDecl.h"
#include "RSymbol/RTypeParamDecl.h"
#include "TranslationContexts.h"
#include "SExpTranslations.h"
#include "SExpToReExp.h"
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
    return funcDecl->GetFuncParam(typeArgs, index);
}

RTypeArguments* MakeTypeArgs(IMatchArgumentsInput* input, RTypeArguments* outerTypeArgs, RTypeArguments* memberTypeArgs, RFactory& rFactory)
{
    size_t partialArgCount = memberTypeArgs->GetCount();
    size_t paramCount = input->GetTypeParamCount();

    // partial typeArgs를 open으로 꽉 채운다
    // [T1, T2, int, T4]
    // 먼저 [int, T4]부터 만든다
    assert(partialArgCount <= paramCount);
    std::vector<RType*> argsItems;
    argsItems.reserve(paramCount);
    for (size_t i = 0; i < partialArgCount; ++i)
        argsItems.push_back(memberTypeArgs->Get(i));
    for (size_t i = partialArgCount; i < paramCount; ++i)
        argsItems.push_back(rFactory.MakeTypeVarType(input->GetTypeParam(i)));
    auto* args = rFactory.MakeTypeArguments(std::move(argsItems));

    // 이제 outer typeArgs와 합친다
    return rFactory.MergeTypeArguments(outerTypeArgs, args);
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

expected<MArgument, DiagPtr> MakeMArgument_Exp(RFuncParameter& funcParam, MExp* exp, TranslationContexts& contexts)
{
    switch (funcParam.kind)
    {
    case RFuncParameterKind::Normal:
        return MArgument_Create{MCreate_BC{exp}};

    case RFuncParameterKind::Ref:
        return Error<Error_Argument_Mismatch_NormalRef_Exp>();

    case RFuncParameterKind::In:
        return MArgument_Loc{
            contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_BC{exp})};

    case RFuncParameterKind::Move:
        return MArgument_Move{MMoveSource_Materialized{
            contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_BC{exp})}};

    case RFuncParameterKind::Forward:
        return MArgument_Forward{MArgument_Forward_RValue{MMoveSource_Materialized{
            contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_BC{exp})}}};

    case RFuncParameterKind::Out:
        // TODO: [54] [out] ref parameter 지원
        throw NotImplementedException{};

    case RFuncParameterKind::Params:
        // TODO: [31] params 구현
        throw NotImplementedException{};

    case RFuncParameterKind::Init:
        // TODO: [27] enumElemDecl에 memberwise ctor 추가하기, memberwise ctor에서 직접 대입 처리
        throw NotImplementedException{};
    }

    unreachable();
}

expected<MArgument, DiagPtr> MakeMArgument_InitExp(RFuncParameter& funcParam, MInitExp* initExp, TranslationContexts& contexts)
{
    switch (funcParam.kind)
    {
    case RFuncParameterKind::Normal:
        return MArgument_Create{MCreate_NBC{initExp}};

    case RFuncParameterKind::Ref:
        return Error<Error_Argument_Mismatch_NormalRef_InitExp>();

    case RFuncParameterKind::In:
        return MArgument_Loc{
            contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_NBC{initExp})};

    case RFuncParameterKind::Move:
        return MArgument_Move{MMoveSource_Materialized{
            contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_NBC{initExp})}};

    case RFuncParameterKind::Forward:
        return MArgument_Forward{MArgument_Forward_RValue{MMoveSource_Materialized{
            contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_NBC{initExp})}}};

    case RFuncParameterKind::Out:
        // TODO: [54] [out] ref parameter 지원
        throw NotImplementedException{};

    case RFuncParameterKind::Params:
        // TODO: [31] params 구현
        throw NotImplementedException{};

    case RFuncParameterKind::Init:
        // TODO: [27] enumElemDecl에 memberwise ctor 추가하기, memberwise ctor에서 직접 대입 처리
        throw NotImplementedException{};
    }

    unreachable();
}

expected<MArgument, DiagPtr> MakeMArgument_LocBC(RFuncParameter& funcParam, MLoc* loc, TranslationContexts& contexts)
{
    switch (funcParam.kind)
    {
    case RFuncParameterKind::Normal:
        return MArgument_Create{MCreate_BC{contexts.mFactory->MakeMExp<MExp_Load>(loc)}};
    case RFuncParameterKind::Ref:
        return MArgument_Loc{loc};
    case RFuncParameterKind::In:
        return MArgument_Loc{loc};
    case RFuncParameterKind::Move:
        return Error<Error_Argument_Mismatch_MoveRef_LocBC>();
    case RFuncParameterKind::Forward:
        return MArgument_Forward{MArgument_Forward_LValue{loc}};
    case RFuncParameterKind::Out:
        // TODO: [54] [out] ref parameter 지원
        throw NotImplementedException{};

    case RFuncParameterKind::Params:
        // TODO: [31] params 구현
        throw NotImplementedException{};

    case RFuncParameterKind::Init:
        // TODO: [27] enumElemDecl에 memberwise ctor 추가하기, memberwise ctor에서 직접 대입 처리
        throw NotImplementedException{};
    }

    unreachable();
}

expected<MArgument, DiagPtr> MakeMArgument_LocNBC(RFuncParameter& funcParam, MLoc* loc, RType* locType, TranslationContexts& contexts)
{
    switch (funcParam.kind)
    {
    case RFuncParameterKind::Normal:
    {
        if (auto* structType = dynamic_cast<RType_Struct*>(locType))
        {
            // TODO: [40] MInitExp_StructCtorKind_*를 쓸때 Copy, Move가 가능한지 확인하고 fallback까지 하는 코드 작성
            auto* initExp = contexts.mFactory->MakeMInitExp<MInitExp_StructCtor>(MInitExp_StructCtorKind_Copy{
                .structType = structType,
                .src = MRead_Loc{.loc = loc}});

            return MArgument_Create{MCreate_NBC{initExp}};
        }

        // TODO: [55] struct 이외의 Non-bitwisecopyable 처리
        throw NotImplementedException{};
    }

    case RFuncParameterKind::Ref:
        return MArgument_Loc{loc};

    case RFuncParameterKind::In:
        return MArgument_Loc{loc};

    case RFuncParameterKind::Move:
        return Error<Error_Argument_Mismatch_MoveRef_LocNBC>();

    case RFuncParameterKind::Forward:
        return MArgument_Forward{MArgument_Forward_LValue{loc}};

    case RFuncParameterKind::Out:
        // TODO: [54] [out] ref parameter 지원
        throw NotImplementedException{};

    case RFuncParameterKind::Params:
        // TODO: [31] params 구현
        throw NotImplementedException{};

    case RFuncParameterKind::Init:
        // TODO: [27] enumElemDecl에 memberwise ctor 추가하기, memberwise ctor에서 직접 대입 처리
        throw NotImplementedException{};
    }

    unreachable();
}

expected<MArgument, DiagPtr> MakeMArgument(RFuncParameter& funcParam, SArgument* sArg, std::vector<TypeEqualConstraint>& constraints, TranslationContexts& contexts)
{
    auto e_reArg = TranslateSExpToReExp(sArg->exp, funcParam.type, contexts);
    RETURN_ON_ERROR(e_reArg);

    // modifier 체크
    if (sArg->o_modifier)
    {
        switch (*sArg->o_modifier)
        {
        case SArgModifier::Ref:
            // ReExp_Loc이 아닌 경우, 에러
            if (!holds_alternative<ReExp_Loc>(*e_reArg))
                return Error<Error_Argument_Ref_ArgIsNotLoc>();

            if (!funcParam.IsRef())
                return Error<Error_Argument_Ref_ParamIsNotRef>();

            break;

        case SArgModifier::Move:
            if (!holds_alternative<ReExp_Loc>(*e_reArg))
                return Error<Error_Argument_Move_ArgIsNotLoc>();

            if (!(funcParam.kind == RFuncParameterKind::Move || funcParam.kind == RFuncParameterKind::Forward))
                return Error<Error_Argument_Move_ParamMismatch>();

            break;

        case SArgModifier::Forward:
            // TODO: [57] [forward] ref parameter 지원
            throw NotImplementedException{};

        case SArgModifier::Out:
            // TODO: [54] [out] ref parameter 지원
            throw NotImplementedException{};

        case SArgModifier::Params:
            // TODO: [31] params 구현
            throw NotImplementedException{};
        }
    }

    return visit([&funcParam, &contexts](auto& reArg) -> expected<MArgument, DiagPtr> {
        using T = remove_cvref_t<decltype(reArg)>;

        if constexpr (same_as<T, ReExp_Loc>)
        {
            RType* type = GetType(reArg.mLoc, &*contexts.rFactory);

            switch (type->GetCopyStrategy())
            {
            case RCopyStrategy::Void: throw RuntimeFatalException{}; // loc을 리턴했는데 void인 경우는 없다
            case RCopyStrategy::Bitwise: return MakeMArgument_LocBC(funcParam, reArg.mLoc, contexts);
            case RCopyStrategy::NonBitwise: return MakeMArgument_LocNBC(funcParam, reArg.mLoc, type, contexts);
            }

            unreachable();
        }
        else if constexpr (same_as<T, ReExp_Exp>)
            return MakeMArgument_Exp(funcParam, reArg.mExp, contexts);

        else if constexpr (same_as<T, ReExp_InitExp>)
            return MakeMArgument_InitExp(funcParam, reArg.mInitExp, contexts);

        else if constexpr (same_as<T, ReExp_StmtCall>)
            return Error<Error_Argument_StmtCall>();

        else if constexpr (same_as<T, ReExp_StmtAssign>)
            return Error<Error_Argument_StmtAssign>();

        else static_assert(false);

    }, *e_reArg);
}

expected<ArgumentsMatch, DiagPtr> MatchArguments(
    IMatchArgumentsInput* input,
    RTypeArguments* outerTypeArgs, 
    RTypeArguments* partialMemberTypeArgs,
    SArguments* sArgs,
    TranslationContexts& contexts)
{
    auto* partialTypeArgs = MakeTypeArgs(input, outerTypeArgs, partialMemberTypeArgs, *contexts.rFactory);

    vector<TypeEqualConstraint> constraints;

    // TODO: 가변인자 처리    
    auto funcParamCount = input->GetFuncParamCount();
    auto argCount = sArgs->items.size();
    if (funcParamCount != argCount)
        return Error<Error_FuncMatch_MismatchBetweenParamCountAndArgCount>();

    vector<MArgument> mArgs;
    mArgs.reserve(argCount); // TODO: 메모리 재사용 최적화
    for (size_t i = 0, count = argCount; i < count; ++i)
    {
        auto* sArgItem = sArgs->items[i];
        auto rFuncParam = input->GetFuncParam(partialTypeArgs, i);

        auto e_mArg = MakeMArgument(rFuncParam, sArgItem, constraints, contexts);
        RETURN_ON_ERROR(e_mArg);

        mArgs.push_back(move(*e_mArg));
    }

    // TODO: [56] constraint resolver 구현
    assert(constraints.empty());

    return ArgumentsMatch{partialTypeArgs, std::move(mArgs)};
}

} // namespace Citron