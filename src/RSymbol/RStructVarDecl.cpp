#include "RStructVarDecl.h"
#include "Infra/Exceptions.h"
#include "RStructDecl.h"

using namespace std;

namespace Citron {

RStructVarDecl::RStructVarDecl(RStructDecl* _struct, RStructMemberAccessor accessor, bool bStatic, RType* declType, TakeRef<RName> name, size_t index)
    : _struct{_struct}, accessor{accessor}, bStatic{bStatic}, declType{declType}, name{name.Take()}, index{index}
{
}

// from RDecl
RDecl* RStructVarDecl::GetOuter()
{
    return _struct;
}

RIdentifier RStructVarDecl::GetIdentifier()
{
    return RIdentifier{name, 0, {}};
}

size_t RStructVarDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParamDecl* RStructVarDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeDecl* RStructVarDecl::GetTypeMember(InRef<RName> name, size_t typeParamCount)
{
    return nullptr;
}

optional<RDeclRes> RStructVarDecl::GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RDeclRes> RStructVarDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron
