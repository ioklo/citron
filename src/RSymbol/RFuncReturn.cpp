#include "RFuncReturn.h"
#include <cassert>
#include "Infra/Exceptions.h"
#include "RFactory.h"

using namespace std;

namespace Citron {

RType* GetType(RFuncReturn& funcRet, RFactory* rFactory)
{
    return visit([rFactory](auto& funcRet) -> RType* {
        using T = remove_cvref_t<decltype(funcRet)>;
        if constexpr (same_as<T, RFuncReturn_None>) return rFactory->MakeVoidType();
        else if constexpr (same_as<T, RFuncReturn_Normal>)
            return funcRet.type;
        else if constexpr (same_as<T, RFuncReturn_NotSet>)
            throw RuntimeFatalException{};
        else static_assert(false);
    }, funcRet);
}

} // namespace Citron