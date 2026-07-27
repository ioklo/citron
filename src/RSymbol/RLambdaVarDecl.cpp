#include "RLambdaVarDecl.h"
#include "Infra/Exceptions.h"
#include "RLambdaDecl.h"
#include "RMember.h"

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

size_t RLambdaVarDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParam* RLambdaVarDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeParam* RLambdaVarDecl::GetTypeParam(InRef<RName> name)
{
    return nullptr;
}

RTypeDecl* RLambdaVarDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RLambdaVarDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron