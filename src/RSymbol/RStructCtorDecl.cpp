#include "RStructCtorDecl.h"
#include "RStructDecl.h"

using namespace std;

namespace Citron {

RStructCtorDecl::RStructCtorDecl(RDeclKey&& key, RStructDecl* _struct, RStructMemberAccessor accessor, RStructCtorKind kind, 
    std::vector<RTypeParam*>&& typeParams, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
    : key{std::move(key)}, _struct{_struct}, accessor{accessor}, kind{kind}
    , genericsComp{}
    , commonFuncDeclComp{/*bSeqFunc*/false}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
    genericsComp.InitTypeParams(move(typeParams));
    commonFuncDeclComp.InitFuncSignature(RFuncReturn_None{}, RThisKind_Static{}, move(funcParameters), bLastParameterVariadic);
}

RDeclKey& RStructCtorDecl::GetDeclKey()
{
    return key;
}

// from RDecl
RDecl* RStructCtorDecl::GetOuter()
{
    return this;
}

RName* RStructCtorDecl::TryGetName()
{
    return nullptr;
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