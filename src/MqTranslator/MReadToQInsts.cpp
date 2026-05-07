#include "MReadToQInsts.h"

#include "Infra/Expected.h"
#include "MIR/MRead.h"
#include "MqTranslationContexts.h"
#include "MqEmitState.h"
#include "MExpToQInsts.h"
#include "MLocToQInsts.h"

using namespace std;

namespace Citron {

expected<MqEmitState<MqReadResult>, DiagPtr> TranslateMReadToQInsts(MRead& mRead, MqTranslationContexts& contexts)
{
    return visit([&contexts](auto& mRead) -> expected<MqEmitState<MqReadResult>, DiagPtr> {
        using T = remove_cvref_t<decltype(mRead)>;
        if constexpr (same_as<T, MRead_Loc>)
        {
            auto e_s_result = TranslateMLocToQInsts(mRead.loc, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_result);

            return ToReadResult(**e_s_result);
        }
        else if constexpr (same_as<T, MRead_Exp>)
        {
            auto e_s_result = TranslateMExpToQInstsForRead(mRead.exp, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_result);

            return **e_s_result;
        }
        else static_assert(false);
    }, mRead);

}

} // namespace Citron