#include "REnumElemVarDecl.h"
#include "Infra/Exceptions.h"
#include "REnumElemDecl.h"
#include "RMember.h"

using namespace std;

namespace Citron {

REnumElemVarDecl::REnumElemVarDecl(RDeclKey&& key, REnumElemDecl* outer, RName&& name)
    : key{std::move(key)}, enumElem{outer}, name{std::move(name)}, declType{nullptr}
{
}

RDeclKey& REnumElemVarDecl::GetDeclKey()
{
    return key;
}

RDecl* REnumElemVarDecl::GetOuter()
{
    return enumElem;
}

RName* REnumElemVarDecl::TryGetName()
{
    return &name;;
}

size_t REnumElemVarDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParam* REnumElemVarDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeParam* REnumElemVarDecl::GetTypeParam(InRef<RName> name)
{
    return nullptr;
}

RTypeDecl* REnumElemVarDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> REnumElemVarDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron