#include "SmTypeResolveScope.h"
#include "RSymbol/RTypeParam.h"
#include "SmTypeRes.h"
#include "SmFuncContext.h"
#include "SmDeclContext.h"

using namespace std;

namespace Citron {

optional<SmTypeRes> SmTypeResolveScope::ResolveTypeIdentifier(InRef<RName> name, RFactory* rFactory)
{
    return visit([&name, rFactory](auto& scope) -> optional<SmTypeRes> {
        using T = remove_cvref_t<decltype(scope)>;
        if constexpr (same_as<T, SmTypeResolveScope_DeclHeader>)
        {
            for (auto* typeParam : scope.typeParams)
                if (typeParam->GetName() == *name)
                    return SmTypeRes_TypeVar{typeParam};

            return scope.outerDeclContext->ResolveTypeIdentifier(name);
        }
        else if constexpr (same_as<T, SmTypeResolveScope_FuncContext>)
        {
            return scope.funcContext->ResolveTypeIdentifier(name);
        }
        else if constexpr (same_as<T, SmTypeResolveScope_DeclContext>)
        {
            return scope.declContext->ResolveTypeIdentifier(name);
        }
        else
            static_assert(false);
    }, v);
}


} // namespace Citron