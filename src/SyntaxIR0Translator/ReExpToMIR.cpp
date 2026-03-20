#include "ReExpToMIR.h"
#include <cassert>
#include "RSymbol/RTypes.h"
#include "MIR/MExp.h"
#include "MIR/MInitExp.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "ReExp.h"
#include "TranslationContexts.h"
#include "DesignatedDiagnostic.h"

using namespace std;

namespace Citron {

expected<MCreate, DiagPtr> TranslateReExpToMCreate(ReExp& reExp, TranslationContexts& contexts)
{
    return visit([&contexts](auto& reExp) -> expected<MCreate, DiagPtr> {

        using T = remove_cvref_t<decltype(reExp)>;

        if constexpr (same_as<T, ReExp_Loc>)
        {
            auto* type = GetType(reExp.mLoc, &*contexts.rFactory);

            auto copyStrategy = type->GetCopyStrategy();
            assert(copyStrategy != RCopyStrategy::Void);

            if (copyStrategy == RCopyStrategy::Bitwise) // BC
            {
                auto* mExp = contexts.mFactory->MakeMExp<MExp_Load>(reExp.mLoc);
                return MCreate_BC{mExp};
            }
            else if (copyStrategy == RCopyStrategy::NonBitwise) // NBC
            {
                if (auto* structType = dynamic_cast<RType_Struct*>(type))
                {
                    // TODO: [40] MInitExp_StructCtorKind_*를 쓸때 Copy, Move가 가능한지 확인하고 fallback까지 하는 코드 작성
                    auto* mInitExp = contexts.mFactory->MakeMInitExp<MInitExp_StructCtor>(MInitExp_StructCtorKind_Copy{structType, MRead_Loc{reExp.mLoc}});
                    return MCreate_NBC{mInitExp};
                }
                else throw NotImplementedException{};
            }

            unreachable();
        }
        else if constexpr (same_as<T, ReExp_Exp>)
        {
            assert(GetType(reExp.mExp, &*contexts.rFactory)->GetCopyStrategy() == RCopyStrategy::Bitwise);
            return MCreate_BC{reExp.mExp};
        }
        else if constexpr (same_as<T, ReExp_InitExp>)
        {
            assert(GetType(reExp.mInitExp, &*contexts.rFactory)->GetCopyStrategy() == RCopyStrategy::NonBitwise);
            return MCreate_NBC{reExp.mInitExp};
        }
        else if constexpr (same_as<T, ReExp_StmtCall> )
        {
            // TODO: [39] SyntaxIR0Translator Error 정리
            throw NotImplementedException{}; // expression's type is void, can't compatible with value
        }
        else if constexpr (same_as<T, ReExp_StmtAssign>)
        {
            // TODO: [39] SyntaxIR0Translator Error 정리
            throw NotImplementedException{}; // expression's type is void, can't compatible with value
        }
        else static_assert(false);

    }, reExp);
}

expected<MRead, DiagPtr> TranslateReExpToMRead(ReExp& reExp, TranslationContexts& contexts)
{
    return visit([&contexts](auto& reExp) -> expected<MRead, DiagPtr> {

        using T = remove_cvref_t<decltype(reExp)>;
        if constexpr (same_as<T, ReExp_Loc>) 
        {
            return MRead_Loc{reExp.mLoc};
        }
        else if constexpr (same_as<T, ReExp_Exp>) 
        {
            assert(GetType(reExp.mExp, &*contexts.rFactory)->GetCopyStrategy() == RCopyStrategy::Bitwise);

            return MRead_Exp{reExp.mExp};
        }
        else if constexpr (same_as<T, ReExp_InitExp>) 
        {
            assert(GetType(reExp.mInitExp, &*contexts.rFactory)->GetCopyStrategy() == RCopyStrategy::NonBitwise);

            auto* mLoc = contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_NBC{reExp.mInitExp});
            return MRead_Loc{mLoc};
        }
        else if constexpr (same_as<T, ReExp_StmtCall>) 
        {
            // TODO: [39] SyntaxIR0Translator Error 정리
            throw NotImplementedException{}; // expression's type is void, can't compatible with value
        }
        else if constexpr (same_as<T, ReExp_StmtAssign>) 
        {
            // TODO: [39] SyntaxIR0Translator Error 정리
            throw NotImplementedException{}; // expression's type is void, can't compatible with value
        }
        else static_assert(false);

    }, reExp);
}

expected<MLoc*, DiagPtr> TranslateReExpToMLoc(ReExp& reExp, bool bMaterializeExp, IDesignatedDiagnostic* notLocationDiag, TranslationContexts& contexts)
{
    return visit([bMaterializeExp, notLocationDiag, &contexts](auto& reExp) -> expected<MLoc*, DiagPtr> {

        using T = remove_cvref_t<decltype(reExp)>;
        if constexpr (same_as<T, ReExp_Loc>) { return reExp.mLoc; }
        else if constexpr (same_as<T, ReExp_Exp>) 
        {
            assert(GetType(reExp.mExp, &*contexts.rFactory)->GetCopyStrategy() == RCopyStrategy::Bitwise);
            if (!bMaterializeExp) return unexpected{notLocationDiag->MakeDiag()};
            return contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_BC{reExp.mExp});
        }
        else if constexpr (same_as<T, ReExp_InitExp>) 
        {
            assert(GetType(reExp.mInitExp, &*contexts.rFactory)->GetCopyStrategy() == RCopyStrategy::NonBitwise);
            if (!bMaterializeExp) return unexpected{notLocationDiag->MakeDiag()};
            return contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_NBC{reExp.mInitExp});
        }
        else if constexpr (same_as<T, ReExp_StmtCall>) 
        {
            // TODO: [39] SyntaxIR0Translator Error 정리
            throw NotImplementedException{}; // expression's type is void, can't compatible with value
        }
        else if constexpr (same_as<T, ReExp_StmtAssign>) 
        {
            // TODO: [39] SyntaxIR0Translator Error 정리
            throw NotImplementedException{}; // expression's type is void, can't compatible with value
        }
        else static_assert(false);

    }, reExp);
}


} // namespace Citron
