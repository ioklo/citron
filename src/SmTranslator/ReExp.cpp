#include "ReExp.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MInitExp.h"

using namespace std;

namespace Citron {

RType* GetType(ReExp& reExp, RFactory* rFactory)
{
    return visit([rFactory](auto& reExp) -> RType* {
        using T = remove_cvref_t<decltype(reExp)>;

        if constexpr (same_as<T, ReExp_Loc>) { return GetType(reExp.mLoc, rFactory); }
        else if constexpr (same_as<T, ReExp_Exp>) { return GetType(reExp.mExp, rFactory); }
        else if constexpr (same_as<T, ReExp_InitExp>) { return GetType(reExp.mInitExp, rFactory); }
        else if constexpr (same_as<T, ReExp_StmtCall>) { return rFactory->MakeVoidType(); }
        else if constexpr (same_as<T, ReExp_StmtAssign>) { return rFactory->MakeVoidType(); }
        else static_assert(false);
    }, reExp);
}

}