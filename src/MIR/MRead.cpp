#include "MRead.h"
#include "MExp.h"
#include "MLoc.h"

using namespace std;

namespace Citron {

RType* GetType(MRead& read, RFactory* rFactory)
{
    return visit([rFactory](auto& read) -> RType* {
        using T = remove_cvref_t<decltype(read)>;

        if constexpr (same_as<T, MRead_BC>)
        {
            return GetType(read.exp, rFactory);
        }
        else if constexpr (same_as<T, MRead_NBC>)
        {
            return GetType(read.loc, rFactory);
        }
        else static_assert(false);
    }, read);
}

} // namespace Citron