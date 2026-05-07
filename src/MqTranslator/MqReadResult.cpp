#include "MqReadResult.h"

using namespace std;

namespace Citron {

MqReadResult ToReadResult(MqLocResult& locResult)
{
    return visit([](auto& loc) -> MqReadResult {
        using T = remove_cvref_t<decltype(loc)>;
        if constexpr (same_as<T, MqLocResult_Slot>) return MqReadResult_Slot{loc.slotIndex};
        else if constexpr (same_as<T, MqLocResult_Ptr>) return MqReadResult_Ptr{loc.slotIndex};
        else static_assert(false);
    }, locResult);
}

} // namespace Citron