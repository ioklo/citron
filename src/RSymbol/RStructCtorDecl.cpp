#include "RStructCtorDecl.h"
#include "RStructDecl.h"

using namespace std;

namespace Citron {

RStructCtorDecl::RStructCtorDecl(RStructDecl* _struct, RStructMemberAccessor accessor, RStructCtorKind kind)
    : _struct{_struct}, accessor{accessor}, kind{kind}
    , genericsComp{}
    , commonFuncDeclComp{/*bSeqFunc*/false}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
}

void RStructCtorDecl::InitFuncParameters(vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    commonFuncDeclComp.InitFuncReturnAndParams(RFuncReturn_None{}, RThisKind_Static{}, move(funcParameters), bLastParameterVariadic);
}

void RStructCtorDecl::InitTypeParams(vector<RTypeParam*>&& typeParams)
{
    return genericsComp.InitTypeParams(move(typeParams));
}

// from RDecl
RDecl* RStructCtorDecl::GetOuter()
{
    return this;
}

RIdentifier RStructCtorDecl::GetIdentifier()
{
    return RIdentifier{RName_Reserved{RName_ReservedName::Ctor}, commonFuncDeclComp.GetParamIds()};
}

size_t RStructCtorDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RStructCtorDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RStructCtorDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RStructCtorDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RStructCtorDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron