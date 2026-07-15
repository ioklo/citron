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

RTypeParamDecl* RClassVarDecl::GetTypeParam(size_t index)
{
    return 0;
}

RTypeDecl* RClassVarDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RDeclRes> RClassVarDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RDeclRes> RClassVarDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

} // namespace Citron