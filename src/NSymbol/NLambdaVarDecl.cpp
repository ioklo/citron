#include "NLambdaVarDecl.h"

#include "Infra/Exceptions.h"
#include "RSymbol/RTypes.h"
#include "NLambdaDecl.h"


using namespace std;

namespace Citron {

NLambdaVarDecl::NLambdaVarDecl(RType* type, const RName& name)
    : type(type), name(name)
{
}

void NLambdaVarDecl::InitLambda(NLambdaDecl* lambda)
{
    this->lambda = lambda;
}

RType* NLambdaVarDecl::GetUnboundDeclType()
{
    return type;
}

NDecl* NLambdaVarDecl::GetNOuter()
{
    return lambda;
}

RDecl* NLambdaVarDecl::GetROuter()
{
    return lambda;
}

RType* NLambdaVarDecl::GetDeclType(RTypeArguments& typeArgs)
{
    return type->Apply(typeArgs);
}

RIdentifier NLambdaVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

RTypeDecl* NLambdaVarDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    return nullptr;
}

optional<RMember> NLambdaVarDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}


optional<RMember> NLambdaVarDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    // VarDecl 하위 declspace에서 identifier를 resolve할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron