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

void RStructCtorDecl::Init(RDeclKey&& key, std::vector<RTypeParam*>&& typeParams, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    o_key.emplace(std::move(key));
    genericsComp.InitTypeParams(move(typeParams));
    commonFuncDeclComp.InitFuncSignature(RFuncReturn_None{}, RThisKind_Static{}, move(funcParameters), bLastParameterVariadic);
}

RDeclKey& RStructCtorDecl::GetDeclKey()
{
    assert(o_key);
    return *o_key;
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