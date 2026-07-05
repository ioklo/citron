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

} // namespace Citron
