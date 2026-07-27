#include "RClassVarDecl.h"
#include "Infra/Exceptions.h"
#include "RClassDecl.h"

using namespace std;

namespace Citron {

RClassVarDecl::RClassVarDecl(RDeclKey&& key, RClassDecl* _class, RClassMemberAccessor accessor, bool bStatic, RType* declType, TakeRef<RName> name)
    : key{std::move(key)}
    , _class{_class}
    , accessor{accessor}
    , bStatic{bStatic}
    , declType{declType}
    , name{name.Take()}
{
}

// from RDecl

RDeclKey& RClassVarDecl::GetDeclKey()
{
    return key;
}

RDecl* RClassVarDecl::GetOuter()
{
    return _class;
}

RName* RClassVarDecl::TryGetName()
{
    return &name;
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