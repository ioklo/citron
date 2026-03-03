#include "MCallable.h"
#include "RSymbol/RGlobalFuncDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/RLambdaDecl.h"

using namespace std;

namespace Citron {

RType* Citron::GetType(MCallable& call)
{
    return visit([](auto& call) -> RType* {
        using T = remove_cvref_t<decltype(call)>;

        if constexpr (same_as<T, MCall_GlobalFunc>)
        {
            return call.decl->GetReturnType(*call.typeArgs);
        }
        else if constexpr (same_as<T, MCall_ClassFunc>)
        {
            return call.decl->GetReturnType(*call.typeArgs);
        }
        else if constexpr (same_as<T, MCall_StructFunc>)
        {
            return call.decl->GetReturnType(*call.typeArgs);
        }
        else if constexpr (same_as<T, MCall_Lambda>)
        {
            return call.decl->GetReturnType(*call.typeArgs);
        }
        else static_assert(false);
    }, call);
}

} // namespace Citron