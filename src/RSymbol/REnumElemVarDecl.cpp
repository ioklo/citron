#include "REnumElemVarDecl.h"
#include "Infra/Exceptions.h"
#include "REnumElemDecl.h"
#include "RMember.h"

using namespace std;

namespace Citron {

REnumElemVarDecl::REnumElemVarDecl(REnumElemDecl* outer, TakeRef<RName> name)
    : enumElem{outer}, name{name.Take()}, declType{nullptr}
{
}

RDecl* REnumElemVarDecl::GetOuter()
{
    return enumElem;
}

RIdentifier REnumElemVarDecl::GetIdentifier()
{
    return RIdentifier{name, {}};
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