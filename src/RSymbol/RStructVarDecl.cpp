#include "RStructVarDecl.h"
#include "Infra/Exceptions.h"
#include "RStructDecl.h"

using namespace std;

namespace Citron {

RStructVarDecl::RStructVarDecl(RDeclKey&& key, RStructDecl* _struct, RStructMemberAccessor accessor, bool bStatic, RType* declType, TakeRef<RName> name, size_t index)
    : key{move(key)}, _struct{_struct}, accessor{accessor}, bStatic{bStatic}, declType{declType}, name{name.Take()}, index{index}
{
}

// from RDecl
RDeclKey& RStructVarDecl::GetDeclKey()
{
    return key;
}

RDecl* RStructVarDecl::GetOuter()
{
    return _struct;
}

RName* RStructVarDecl::TryGetName()
{
    return &name;
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
