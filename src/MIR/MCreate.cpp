#include "MCreate.h"
#include "RSymbol/RStructCtorDecl.h"
#include "MExp.h"
#include "MInitExp.h"

using namespace std;

namespace Citron {

RType* GetType(MCreate& create, RFactory* rFactory)
{
    return visit([rFactory](auto& create) -> RType*
    {
        using T = remove_cvref_t<decltype(create)>;

        if constexpr (same_as<T, MCreate_Bitwise>)
        {
            return GetType(create.exp, rFactory);
        }
        else if constexpr (same_as<T, MCreate_Init>)
        {
            return GetType(create.initExp, rFactory);
        }
        else static_assert(false);

    }, create);
}

} // namespace Citron