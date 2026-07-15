#include "RClassDecl.h"
#include "Infra/Exceptions.h"
#include "RFactory.h"

using namespace std;

namespace Citron {

RClassDecl::RClassDecl(RTypeDeclOuter outer, TakeRef<RName> name, TakeRef<RFactoryPtr> rFactory)
    : outer{outer}, name{name.Take()}, rFactory{rFactory.Take()}, trivialCtorIndex{-1}
    , genericsComp{}, typeDeclContainerComp{}
{
}

optional<RDeclRes_ClassVar> RClassDecl::ResolveVar(RTypeArguments* typeArgs, InRef<RName> name)
{
    auto i = varsMap.find(*name);
    if (i == varsMap.end()) return nullopt;

    return RDeclRes_ClassVar(i->second, typeArgs);
}

RDecl* RClassDecl::GetOuter()
{
    return outer.GetDecl();
}

RIdentifier RClassDecl::GetIdentifier()
{
    return RIdentifier{name, {}};
}

size_t RClassDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParamDecl* RClassDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeDecl* RClassDecl::GetTypeMember(InRef<RName> name)
{
    if (auto* typeDecl = genericsComp.GetTypeMember(name))
        return typeDecl;

    return typeDeclContainerComp.GetTypeMember(name);
}

optional<RDeclRes> RClassDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    vector<RDeclRes> candidates;

    // type
    if (auto o_type = typeDeclContainerComp.ResolveTypeMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(move(*o_type));

    // class member func
    if (auto o_func = funcDeclContainerComp.GetMemberFunc(typeArgs, name, explicitTypeParamsExceptOuterCount))
        candidates.push_back(move(*o_func));

    if (explicitTypeParamsExceptOuterCount == 0)
        if (auto o_var = ResolveVar(typeArgs, name))
            candidates.push_back(move(*o_var));

    if (candidates.empty()) return nullopt;

    if (1 < candidates.size())
    {
        // TODO: 여러 candidate가 있다고 로깅하고 FatalException던지기
        throw NotImplementedException();
    }

    return move(candidates[0]);
}

optional<RDeclRes> RClassDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = genericsComp.ResolveTypeParam(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    auto* typeArgs = MakeOpenTypeArgs(*rFactory);
    if (auto o_member = ResolveMember(typeArgs, name, explicitTypeParamsExceptOuterCount))
        return o_member;

    // TODO: [37] class base에서도 검색하기
    return outer.GetDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

RDecl* RClassDecl::RTypeDecl_GetDecl()
{
    return this;
}

RType* RClassDecl::GetOpenType()
{
    return rFactory->MakeClassType(this, MakeOpenTypeArgs(*rFactory));
}

RDeclRes RClassDecl::ToRDeclRes(RTypeArguments* typeArgs)
{
    return RDeclRes_Class(typeArgs, this);
}

void RClassDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron