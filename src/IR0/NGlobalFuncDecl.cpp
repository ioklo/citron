#include "NGlobalFuncDecl.h"
#include <cassert>

#include "NNamespaceDecl.h"
#include "Infra/Exceptions.h"

using namespace std;

namespace Citron {

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

optional<RMember> NGlobalFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, IR0Factory& factory)
{
    size_t baseTypeParamCount = outer->GetRDecl()->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;

    return outer->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

}