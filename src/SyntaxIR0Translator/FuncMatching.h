#pragma once

#include <memory>
#include <span>
#include <optional>
#include <vector>
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Transaction.h"
#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RFuncDecl.h"
#include "MIR/MArgument.h"
#include "SExpTranslations.h"
#include "DesignatedDiagnostic.h"
#include "TranslationContexts.h"
#include "ScopeContext.h"
#include "Misc.h"

namespace Citron {

template<typename TDecl>
struct DeclWithOuterTypeArgs;

class RTypeArguments;
struct RFuncParameter;

template<typename TFuncDecl>
struct FuncMatch
{
    TFuncDecl* funcDecl;
    RTypeArguments* typeArgs;    // 전체 typeArgs (outer(open) + func(closed))
    std::vector<MArgument> args;
};

struct ArgumentsMatch
{
    RTypeArguments* typeArgs; // 전체 typeArgs (outer(open) + func(closed))
    std::vector<MArgument> args;
};

class IMatchArgumentsInput
{
public:
    ~IMatchArgumentsInput() = default;

    virtual size_t GetTypeParamCount() = 0;
    virtual RTypeParamDecl* GetTypeParam(size_t index) = 0;

    virtual size_t GetFuncParamCount() = 0;
    virtual RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index) = 0;
};

class RFuncDeclMatchArgumentsInput : public IMatchArgumentsInput
{
    RFuncDecl* funcDecl;

public:
    RFuncDeclMatchArgumentsInput(RFuncDecl* funcDecl) : funcDecl{funcDecl} {}
    virtual size_t GetTypeParamCount() override;
    virtual RTypeParamDecl* GetTypeParam(size_t index) override;

    virtual size_t GetFuncParamCount() override;
    virtual RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index) override;
};

std::expected<ArgumentsMatch, DiagPtr> MatchArguments(
    IMatchArgumentsInput* input,
    RTypeArguments* outerTypeArgs, 
    RTypeArguments* partialMemberTypeArgs,
    SArguments* sArgs,
    TranslationContexts& contexts);

// infos는 한개 이상이어야 한다
// 한개
// struct S<T1> { struct U<T2> { void F<T3, T4>(); void F<T3, T4>(int); } } 환경에서 F<int>(...) 호출시
template<typename TFuncDecl> requires std::derived_from<TFuncDecl, RFuncDecl>
std::expected<FuncMatch<TFuncDecl>, DiagPtr> MatchFunc(
    std::span<DeclWithOuterTypeArgs<TFuncDecl>> infos, // { S<>.U<>.F<,> ... }, [T1, T2] // open type
    RTypeArguments* partialMemberTypeArgs, // [int], closed type, T4는 확정 해야 함
    SArguments* sArgs, 
    TranslationContexts& contexts)
{
    assert(!infos.empty());    
    
    if (infos.size() == 1)
    {   
        auto& info = infos.front();

        RFuncDeclMatchArgumentsInput input{info.decl};
        auto e_argMatch = MatchArguments(&input, info.outerTypeArgs, partialMemberTypeArgs, sArgs, contexts);
        RETURN_ON_ERROR_REFDECL(e_argMatch, argMatch); // argument mismatch인 경우
        
        return FuncMatch<TFuncDecl>(info.decl, argMatch.typeArgs, std::move(argMatch.args));
    }

    std::vector<size_t> candidates;
    std::vector<DiagPtr> diags;
    for (size_t i = 0, count = infos.size(); i < count; i++)
    {
        auto& info = infos[i];
        Transaction transaction(*contexts.scopeContext);

        RFuncDeclMatchArgumentsInput input{info.decl};
        auto e_argMatch = MatchArguments(&input, info.outerTypeArgs, partialMemberTypeArgs, sArgs, contexts);

        // TODO: [58] FuncMatcher 에러 개선
        if (!e_argMatch)
            diags.push_back(move(e_argMatch.error()));
        else
            candidates.push_back(i);

        transaction.Rollback();
    }

    if (candidates.empty())
        return Error<AggregateDiag>(std::move(diags));

    if (1 < candidates.size())
        return Error<Error_FuncMatch_MultipleCandidates>();

    // 롤백했기 때문에 다시 계산
    auto& info = infos[candidates.front()];
    RFuncDeclMatchArgumentsInput input{info.decl};
    auto e_argMatch = MatchArguments(&input, info.outerTypeArgs, partialMemberTypeArgs, sArgs, contexts);
    assert(e_argMatch);
    return FuncMatch<TFuncDecl>(info.decl, e_argMatch->typeArgs, std::move(e_argMatch->args));
}

} // namespace Citron

