#include "REnumElemVarDecl.h"
#include "Infra/Exceptions.h"
#include "REnumElemDecl.h"

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
    return RIdentifier{name, 0, {}};
}

size_t REnumElemVarDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParamDecl* REnumElemVarDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeDecl* REnumElemVarDecl::GetTypeMember(InRef<RName> name, size_t typeParamCount)
{
    return nullptr;
}

optional<RDeclRes> REnumElemVarDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RDeclRes> REnumElemVarDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron