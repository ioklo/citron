#include "NClassVarDecl.h"

#include <Infra/Exceptions.h>
#include "NClassDecl.h"
#include "RTypeFactory.h"

using namespace std;

namespace Citron {

NDecl* NClassVarDecl::GetNOuter()
{
    return _class.lock().get();
}

RDecl* NClassVarDecl::GetROuter()
{
    return _class.lock().get();
}

RIdentifier NClassVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

RTypePtr NClassVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return declType->Apply(typeArgs, factory);
}

optional<RMember> NClassVarDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RMember> NClassVarDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron