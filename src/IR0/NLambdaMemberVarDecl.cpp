#include "NLambdaMemberVarDecl.h"

#include <Infra/Exceptions.h>
#include "NLambdaDecl.h"

using namespace std;

namespace Citron {

NLambdaMemberVarDecl::NLambdaMemberVarDecl(const RTypePtr& type, const RName& name)
    : type(type), name(name)
{
}

void NLambdaMemberVarDecl::InitLambda(const std::shared_ptr<NLambdaDecl>& lambda)
{
    this->lambda = lambda;
}

RTypePtr NLambdaMemberVarDecl::GetUnboundDeclType()
{
    return type;
}

NDecl* NLambdaMemberVarDecl::GetNOuter()
{
    return lambda.lock().get();
}

RDecl* NLambdaMemberVarDecl::GetROuter()
{
    return lambda.lock().get();
}

RTypePtr NLambdaMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return type->Apply(typeArgs, factory);
}

RIdentifier NLambdaMemberVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

optional<RMember> NLambdaMemberVarDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}


optional<RMember> NLambdaMemberVarDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    // MemberVarDecl 하위 declspace에서 identifier를 resolve할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron