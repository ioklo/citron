#include "NGlobalFuncDecl.h"
#include <cassert>

#include "NNamespaceDecl.h"
#include "Infra/Exceptions.h"

using namespace std;

namespace Citron {

NDecl* NGlobalFuncDecl::GetNOuter()
{
    return outer.lock().get();
}

RDecl* NGlobalFuncDecl::GetROuter()
{
    return outer.lock().get();
}

RIdentifier NGlobalFuncDecl::GetIdentifier()
{
    throw NotImplementedException{};
}

optional<RMember> NGlobalFuncDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    // 람다는 검색시키지 않는다
    // 현재 함수에서 Declaration을 할 수 없기 때문에 
    return nullopt;
}

optional<RMember> NGlobalFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto sharedOuter = outer.lock();
    assert(sharedOuter);

    size_t baseTypeParamCount = sharedOuter->GetRDecl()->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;

    return sharedOuter->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

}