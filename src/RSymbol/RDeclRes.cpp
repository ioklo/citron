#include "RDeclRes.h"

#include "Infra/Variants.h"

#include "RGlobalFuncDecl.h"
#include "RClassFuncDecl.h"
#include "RStructFuncDecl.h"
#include "DeclWithOuterTypeArgs.h"

using namespace std;

namespace Citron {

RDeclRes_GlobalFuncs::RDeclRes_GlobalFuncs(vector<DeclWithOuterTypeArgs<RGlobalFuncDecl*>>&& items)
    : items{move(items)}
{
}

RDeclRes_GlobalFuncs::RDeclRes_GlobalFuncs(const RDeclRes_GlobalFuncs& member) = default;
RDeclRes_GlobalFuncs::~RDeclRes_GlobalFuncs() = default;

RDeclRes_ClassFuncs::RDeclRes_ClassFuncs(vector<DeclWithOuterTypeArgs<RClassFuncDecl*>>&& items)
    : items{move(items)}
{
}

RDeclRes_ClassFuncs::RDeclRes_ClassFuncs(const RDeclRes_ClassFuncs&) = default;
RDeclRes_ClassFuncs::~RDeclRes_ClassFuncs() = default;

RDeclRes_StructFuncs::RDeclRes_StructFuncs(vector<DeclWithOuterTypeArgs<RStructFuncDecl*>>&& items)
    : items{move(items)}
{

}

RDeclRes_StructFuncs::RDeclRes_StructFuncs(const RDeclRes_StructFuncs&) = default;
RDeclRes_StructFuncs::~RDeclRes_StructFuncs() = default;


template<typename TRFuncDecl>
vector<DeclWithOuterTypeArgs<RFuncDecl>> GetItems(vector<DeclWithOuterTypeArgs<TRFuncDecl>>& items)
{
    vector<DeclWithOuterTypeArgs<RFuncDecl>> result;
    result.reserve(items.size());

    for (auto& item : items)
        result.emplace_back(item.decl, item.outerTypeArgs);

    return result;
}

vector<DeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs(RDeclRes& member)
{
    return visit([](auto& member) -> vector<DeclWithOuterTypeArgs<RFuncDecl>> {
        using T = remove_cvref_t<decltype(member)>;

        if constexpr (same_as<T, RDeclRes_GlobalFuncs>) { return GetItems(member.items); }
        else if constexpr (same_as<T, RDeclRes_ClassFuncs>) { return GetItems(member.items); }
        else if constexpr (same_as<T, RDeclRes_StructFuncs>) { return GetItems(member.items); }
        else { return {}; }

    }, member);
}

} // namespace Citron
