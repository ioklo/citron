#include "RClassCtorDecl.h"
#include "RClassDecl.h"

using namespace std;

namespace Citron {

RClassCtorDecl::RClassCtorDecl(RClassDecl* _class, RClassMemberAccessor accessor, bool bTrivial)
    : _class{_class}
    , accessor{accessor}
    , bTrivial{bTrivial}
    , genericsComp{}
    , commonFuncDeclComp{/*bSeqFunc*/false}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
}

void RClassCtorDecl::InitTypeParams(vector<RTypeParam*>&& typeParams)
{
    return genericsComp.InitTypeParams(move(typeParams));
}

RDecl* RClassCtorDecl::GetOuter()
{
    return _class;
}

RIdentifier RClassCtorDecl::GetIdentifier()
{
    return RIdentifier{RName_Reserved{RName_ReservedName::Ctor}, commonFuncDeclComp.GetParamIds()};
}

size_t RClassCtorDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RClassCtorDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RClassCtorDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RClassCtorDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RClassCtorDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron