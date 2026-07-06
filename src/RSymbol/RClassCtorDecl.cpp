#include "RClassCtorDecl.h"
#include "RClassDecl.h"

using namespace std;

namespace Citron {

RClassCtorDecl::RClassCtorDecl(RClassDecl* _class, RClassMemberAccessor accessor, bool bTrivial)
    : _class{_class}
    , accessor{accessor}
    , bTrivial{bTrivial}
    , genericsComp{}
    , commonFuncDeclComp{/*bSeqFunc*/false}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
}

RDecl* RClassCtorDecl::GetOuter()
{
    return _class;
}

RIdentifier RClassCtorDecl::GetIdentifier()
{
    return RIdentifier{RName_Reserved{RName_ReservedName::Ctor}, 0, commonFuncDeclComp.GetParamIds()};
}

size_t RClassCtorDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParamDecl* RClassCtorDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeDecl* RClassCtorDecl::GetTypeMember(InRef<RName> name, size_t typeParamCount)
{
    return genericsComp.GetTypeMember(name, typeParamCount);
}

std::optional<RDeclRes> RClassCtorDecl::GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RDeclRes> RClassCtorDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = genericsComp.ResolveIdentifierCore(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (auto o_member = commonFuncDeclComp.ResolveIdentifierCore(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _class->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

} // namespace Citron