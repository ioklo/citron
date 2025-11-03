#include "NGlobalFuncDecl.h"
#include <cassert>

#include "NNamespaceDecl.h"
#include "Infra/Exceptions.h"

using namespace std;

namespace Citron {

NGlobalFuncDecl::NGlobalFuncDecl(NNamespaceDecl* outer, RAccessor accessor, bool bStatic, bool bSeqFunc, RName&& rName, std::vector<std::string>&& typeParams)
    : NCommonFuncDeclComponent(bStatic, bSeqFunc, std::move(typeParams))
    , outer{outer}
    , accessor{accessor}
    , name{std::move(rName)}
{
}

NDecl* NGlobalFuncDecl::GetNOuter()
{
    return outer;
}

RDecl* NGlobalFuncDecl::GetROuter()
{
    return outer;
}

RIdentifier NGlobalFuncDecl::GetIdentifier()
{
    throw NotImplementedException{};
}

optional<RMember> NGlobalFuncDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    // 람다는 검색시키지 않는다
    // 현재 함수에서 Declaration을 할 수 없기 때문에 
    return nullopt;
}

optional<RMember> NGlobalFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory)
{
    size_t baseTypeParamCount = outer->GetRDecl()->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;

    return outer->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

}