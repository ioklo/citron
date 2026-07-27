#include "RTraitFuncDecl.h"
#include "RTraitDecl.h"
#include "RMember.h"

using namespace std;

namespace Citron {

RTraitFuncDecl::RTraitFuncDecl(RTraitDecl* trait, bool bStatic, RName&& name)
    : trait{trait}, bStatic{bStatic}, name{std::move(name)}
    , genericsComp{}
{
}

void RTraitFuncDecl::Init(RDeclKey&& key, vector<RTypeParam*>&& typeParams, RFuncReturn&& funcReturn, vector<RFuncParameter>&& funcParameters, bool bLastParamVariadic)
{
    o_lazyInit.emplace(std::move(key), move(typeParams), move(funcReturn), move(funcParameters), bLastParamVariadic);
}

RDeclKey& RTraitFuncDecl::GetDeclKey()
{
    assert(o_lazyInit);
    return o_lazyInit->key;
}

RDecl* RTraitFuncDecl::GetOuter()
{
    return trait;
}

RName* RTraitFuncDecl::TryGetName()
{
    return &name;
}

size_t RTraitFuncDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RTraitFuncDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RTraitFuncDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RTraitFuncDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RTraitFuncDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron