#include "NFuncDecl.h"
#include "RSymbol/RFuncDecl.h"
#include "NGlobalFuncDecl.h"
#include "NClassCtorDecl.h"
#include "NClassFuncDecl.h"
#include "NStructCtorDecl.h"
#include "NStructDtorDecl.h"
#include "NStructFuncDecl.h"
#include "NLambdaDecl.h"

using namespace std;

namespace Citron {

NDecl* NFuncDecl::GetNDecl()
{
    return visit([](auto* funcDecl) -> NDecl* { return funcDecl; }, v);
}

RFuncDecl NFuncDecl::GetRFuncDecl()
{
    return visit([](auto* nFuncDecl) -> RFuncDecl { return nFuncDecl; }, v);
}

bool NFuncDecl::IsSeqFunc()
{
    return visit([](auto* funcDecl) -> bool { return funcDecl->IsSeqFunc(); }, v);
}

NFuncDeclOuter NFuncDecl::GetNFuncDeclOuter()
{
    return visit([](auto* funcDecl) -> NFuncDeclOuter {
        using T = remove_cvref_t<decltype(funcDecl)>;

        if constexpr (same_as<T, NGlobalFuncDecl*>)
            return funcDecl->outer;
        else if constexpr (same_as<T, NClassCtorDecl*>)
            return funcDecl->_class;
        else if constexpr (same_as<T, NClassFuncDecl*>)
            return funcDecl->_class;
        else if constexpr (same_as<T, NStructFuncDecl*>)
            return funcDecl->_struct;
        else if constexpr (same_as<T, NStructCtorDecl*>)
            return funcDecl->_struct;
        else if constexpr (same_as<T, NStructDtorDecl*>)
            return funcDecl->_struct;
        else if constexpr (same_as<T, NLambdaDecl*>)
            return funcDecl->outer;
        else static_assert(false);
    }, v);
}

} // namespace Citron