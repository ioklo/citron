#include "RTraitFuncDecl.h"
#include "RTraitDecl.h"

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

    return RIdentifier{name, genericsComp.GetTypeParamCount(), move(paramIds)};
}

size_t RTraitFuncDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParamDecl* RTraitFuncDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeDecl* RTraitFuncDecl::GetTypeMember(InRef<RName> name, size_t typeParamCount)
{
    return genericsComp.GetTypeMember(name, typeParamCount);
}

optional<RDeclRes> RTraitFuncDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto* typeDecl = genericsComp.GetTypeMember(name, explicitTypeParamsExceptOuterCount))
        return typeDecl->ToRDeclRes(typeArgs);

    return nullopt;
}

optional<RDeclRes> RTraitFuncDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = genericsComp.ResolveTypeParam(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return trait->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

} // namespace Citron