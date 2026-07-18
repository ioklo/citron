#include "RClassVarDecl.h"
#include "Infra/Exceptions.h"
#include "RClassDecl.h"

using namespace std;

namespace Citron {

RClassVarDecl::RClassVarDecl(RClassDecl* _class, RClassMemberAccessor accessor, bool bStatic, RType* declType, TakeRef<RName> name)
    : _class{_class}
    , accessor{accessor}
    , bStatic{bStatic}
    , declType{declType}
    , name{name.Take()}
{
}

RDecl* RClassVarDecl::GetOuter()
{
    return _class;
}

RIdentifier RClassVarDecl::GetIdentifier()
{
    return RIdentifier{name, {}};
}

size_t RClassVarDecl::GetTypeParamCount()
{
    return size_t();
}

RTypeParam* RClassVarDecl::GetTypeParam(size_t index)
{
    return 0;
}

RTypeParam* RClassVarDecl::GetTypeParam(InRef<RName> name)
{
    return nullptr;
}

RTypeDecl* RClassVarDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RClassVarDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron