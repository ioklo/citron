#include "RLambdaVarDecl.h"
#include "Infra/Exceptions.h"
#include "RLambdaDecl.h"

using namespace std;

namespace Citron {

RLambdaVarDecl::RLambdaVarDecl(RType* type, TakeRef<RName> name)
    : lambda{nullptr}
    , type{type}
    , name{name.Take()}
{   
}

void RLambdaVarDecl::InitLambda(RLambdaDecl* lambda)
{
    this->lambda = lambda;
}

RDecl* RLambdaVarDecl::GetOuter()
{
    assert(lambda);
    return lambda;
}

RIdentifier RLambdaVarDecl::GetIdentifier()
{
    return RIdentifier{name, 0, {}};
}

size_t RLambdaVarDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParamDecl* RLambdaVarDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeDecl* RLambdaVarDecl::GetTypeMember(InRef<RName> name, size_t typeParamCount)
{
    return nullptr;
}

optional<RDeclRes> RLambdaVarDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RDeclRes> RLambdaVarDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    // VarDecl 하위 declspace에서 identifier를 resolve할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron