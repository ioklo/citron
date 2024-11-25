#include "NLambdaVarDecl.h"

#include <Infra/Exceptions.h>
#include "NLambdaDecl.h"

using namespace std;

namespace Citron {

NLambdaVarDecl::NLambdaVarDecl(const RTypePtr& type, const RName& name)
    : type(type), name(name)
{
}

void NLambdaVarDecl::InitLambda(const std::shared_ptr<NLambdaDecl>& lambda)
{
    this->lambda = lambda;
}

RTypePtr NLambdaVarDecl::GetUnboundDeclType()
{
    return type;
}

NDecl* NLambdaVarDecl::GetNOuter()
{
    return lambda.lock().get();
}

RDecl* NLambdaVarDecl::GetROuter()
{
    return lambda.lock().get();
}

RTypePtr NLambdaVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return type->Apply(typeArgs, factory);
}

RIdentifier NLambdaVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

optional<RMember> NLambdaVarDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}


optional<RMember> NLambdaVarDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    // VarDecl 하위 declspace에서 identifier를 resolve할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron