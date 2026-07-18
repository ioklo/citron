#include "RTraitFuncDecl.h"
#include "RTraitDecl.h"
#include "RMember.h"

using namespace std;

namespace Citron {

RTraitFuncDecl::RTraitFuncDecl(RTraitDecl* trait, bool bStatic, RFuncReturn&& funcReturn, RName&& name, vector<RFuncParameter>&& funcParameters, bool bLastParamVariadic)
    : trait{trait}, bStatic{bStatic}, funcReturn{move(funcReturn)}, name{move(name)}, funcParameters{move(funcParameters)}, bLastParamVariadic{bLastParamVariadic}
    , genericsComp{}
{
}

RDecl* RTraitFuncDecl::GetOuter()
{
    return trait;
}

RIdentifier RTraitFuncDecl::GetIdentifier()
{
    vector<RType*> paramIds;
    for (auto& param : funcParameters)
        paramIds.push_back(param.type);

    return RIdentifier{name, move(paramIds)};
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