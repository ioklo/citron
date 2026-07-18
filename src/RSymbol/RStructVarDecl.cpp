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
    return RIdentifier{name, {}};
}

size_t RStructVarDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParam* RStructVarDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeParam* RStructVarDecl::GetTypeParam(InRef<RName> name)
{
    return nullptr;
}

RTypeDecl* RStructVarDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RStructVarDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron
