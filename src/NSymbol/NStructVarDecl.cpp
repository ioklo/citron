#include "NStructVarDecl.h"

#include <cassert>
#include "Infra/Exceptions.h"

#include "RSymbol/RTypes.h"
#include "NStructDecl.h"

using namespace std;

namespace Citron {

NStructVarDecl::NStructVarDecl(NStructDecl* _struct, RAccessor accessor, bool bStatic, const std::string& name, RType* declType, size_t index)
    : _struct{_struct}
    , accessor{accessor}
    , bStatic{bStatic}
    , name{name}
    , declType{declType}
    , index{index}
{
}

RType* NStructVarDecl::GetUnboundDeclType()
{
    assert(declType);
    return declType;
}

NDecl* NStructVarDecl::GetNOuter()
{
    return _struct;
}

RDecl* NStructVarDecl::GetROuter()
{
    return _struct;
}

RIdentifier NStructVarDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

RTypeDecl* NStructVarDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    return nullptr;
}

optional<RMember> NStructVarDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<Citron::RMember> NStructVarDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

RType* NStructVarDecl::GetDeclType(RTypeArguments& typeArgs, RFactory& factory)
{
    assert(declType != nullptr);
    return declType->Apply(typeArgs, factory);
}


} // namespace Citron