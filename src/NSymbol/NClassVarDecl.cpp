#include "NClassVarDecl.h"

#include "Infra/Exceptions.h"
#include "NClassDecl.h"

using namespace std;

namespace Citron {

NDecl* NClassVarDecl::GetNOuter()
{
    return _class;
}

RDecl* NClassVarDecl::GetROuter()
{
    return _class;
}

RIdentifier NClassVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

RTypeDecl* NClassVarDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    return nullptr;
}

RType* NClassVarDecl::GetDeclType(RTypeArguments& typeArgs)
{
    return declType->Apply(typeArgs);
}

optional<RMember> NClassVarDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RMember> NClassVarDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron