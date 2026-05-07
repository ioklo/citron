#include "MCreateToQInsts.h"
#include "MExpToQInsts.h"
#include "MInitExpToQInsts.h"

using namespace std;

namespace Citron {

expected<MqEmitState<void>, DiagPtr> TranslateMCreateToQInsts(MCreate& create, MqCreateTarget createTarget, MqTranslationContexts& contexts)
{
    return visit([&createTarget, &contexts](auto& create) -> expected<MqEmitState<void>, DiagPtr> {
        using T = remove_cvref_t<decltype(create)>;

        if constexpr (same_as<T, MCreate_BC>)
            return TranslateMExpToQInstsForCreate(create.exp, createTarget, contexts);
        else if constexpr (same_as<T, MCreate_NBC>)
            return TranslateMInitExpToQInsts(create.initExp, createTarget, contexts);
        else static_assert(false);
    }, create);
}

} // namespace Citron