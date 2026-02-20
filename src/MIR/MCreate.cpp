#include "MCreate.h"
#include "RSymbol/RStructCtorDecl.h"
#include "MExp.h"

using namespace std;

namespace Citron {

RType* GetType(MCreate& create)
{
    return visit([](auto& create) -> RType*
    {
        using T = remove_cvref_t<decltype(create)>;

        if constexpr (same_as<T, MCreate_Bitwise>)
        {
            return create.exp->GetType();
        }
        // MCreate_StructCopyCtor, MCreate_StructMoveCtor, MCreate_StructCtor, MCreate_RVO>;
        else if constexpr (same_as<T, MCreate_StructCopyCtor> || same_as<T, MCreate_StructMoveCtor> || same_as<T, MCreate_StructCtor>)
        {
            return create.type;
        }
        else if constexpr (same_as<T, MCreate_RVO>)
        {
            return create.callExp->GetType();
        }
        else static_assert(false);

    }, create);
}

} // namespace Citron