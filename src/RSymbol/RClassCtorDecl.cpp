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

void RClassCtorDecl::InitTypeParams(std::vector<RTypeParamDecl*>&& typeParams)
{
    return genericsComp.InitTypeParams(move(typeParams));
}

RDecl* RClassCtorDecl::GetOuter()
{
    return _class;
}

RIdentifier RClassCtorDecl::GetIdentifier()
{
    return RIdentifier{RName_Reserved{RName_ReservedName::Ctor}, commonFuncDeclComp.GetParamIds()};
}

size_t RClassCtorDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParamDecl* RClassCtorDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeDecl* RClassCtorDecl::GetTypeMember(InRef<RName> name)
{
    return genericsComp.GetTypeMember(name);
}

std::optional<RDeclRes> RClassCtorDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RDeclRes> RClassCtorDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = genericsComp.ResolveTypeParam(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (auto o_member = commonFuncDeclComp.ResolveFuncParam(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _class->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

} // namespace Citron