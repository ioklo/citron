#include "RDeclRes.h"

#include "Infra/Variants.h"

#include "RGlobalFuncDecl.h"
#include "RClassFuncDecl.h"
#include "RStructFuncDecl.h"
#include "DeclWithOuterTypeArgs.h"

using namespace std;

namespace Citron {

RDeclRes_GlobalFuncs::RDeclRes_GlobalFuncs(vector<TDeclWithOuterTypeArgs<RGlobalFuncDecl>>&& items)
    : items{move(items)}
{
}

RDeclRes_GlobalFuncs::RDeclRes_GlobalFuncs(const RDeclRes_GlobalFuncs& member) = default;
RDeclRes_GlobalFuncs::~RDeclRes_GlobalFuncs() = default;

RDeclRes_ClassFuncs::RDeclRes_ClassFuncs(vector<TDeclWithOuterTypeArgs<RClassFuncDecl>>&& items)
    : items{move(items)}
{
}

RDeclRes_ClassFuncs::RDeclRes_ClassFuncs(const RDeclRes_ClassFuncs&) = default;
RDeclRes_ClassFuncs::~RDeclRes_ClassFuncs() = default;

RDeclRes_StructFuncs::RDeclRes_StructFuncs(vector<TDeclWithOuterTypeArgs<RStructFuncDecl>>&& items)
    : items{move(items)}
{

}

RDeclRes_StructFuncs::RDeclRes_StructFuncs(const RDeclRes_StructFuncs&) = default;
RDeclRes_StructFuncs::~RDeclRes_StructFuncs() = default;


template<typename TRFuncDecl>
vector<DeclWithOuterTypeArgs> GetItems(vector<TDeclWithOuterTypeArgs<TRFuncDecl>>& items)
{
    vector<DeclWithOuterTypeArgs> result;
    result.reserve(items.size());

    for (auto& item : items)
        result.emplace_back(DeclWithOuterTypeArgs{item.decl, item.outerTypeArgs});

    return result;
}

vector<DeclWithOuterTypeArgs> RDeclRes::GetFuncDeclWithOuterTypeArgs()
{
    return visit([](auto& member) -> vector<DeclWithOuterTypeArgs> {
        using T = remove_cvref_t<decltype(member)>;

        if constexpr (same_as<T, RDeclRes_GlobalFuncs>) { return GetItems(member.items); }
        else if constexpr (same_as<T, RDeclRes_ClassFuncs>) { return GetItems(member.items); }
        else if constexpr (same_as<T, RDeclRes_StructFuncs>) { return GetItems(member.items); }
        else { return {}; }

    }, v);
}

} // namespace Citron
