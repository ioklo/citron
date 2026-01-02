#include "NClassFuncDecl.h"

#include <cassert>
#include "Infra/Exceptions.h"

#include "NClassDecl.h"

using namespace std;

namespace Citron {

NDecl* NClassFuncDecl::GetNOuter()
{
    return _class;
}

NFuncDeclOuter* NClassFuncDecl::GetNFuncDeclOuter()
{
    return _class;
}

RDecl* NClassFuncDecl::GetROuter()
{
    return _class;
}

RIdentifier NClassFuncDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), NCommonFuncDeclComponent::GetParamIds() };
}

RTypeDecl* NClassFuncDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    // TODO: [26] typeParams에서도 검색해야 함
    return nullptr;
}

optional<RMember> NClassFuncDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RMember> NClassFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    size_t baseTypeParamCount = _class->GetAllTypeParamCount();
    if (auto o_member = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _class->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

} // namespace Citron