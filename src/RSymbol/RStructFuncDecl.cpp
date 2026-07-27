#include "RStructFuncDecl.h"
#include "RStructDecl.h"

using namespace std;

namespace Citron {

RStructFuncDecl::RStructFuncDecl(RStructDecl* _struct, RStructMemberAccessor accessor, RName&& name, bool bSeqFunc)
    : _struct{_struct}, accessor{accessor}, name{move(name)}
    , genericsComp{}
    , commonFuncDeclComp{bSeqFunc}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
}

void RStructFuncDecl::Init(RDeclKey&& key, bool bStatic, RFuncReturn&& funcRet, std::vector<RTypeParam*>&& typeParams, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    o_key = std::move(key);
    genericsComp.InitTypeParams(move(typeParams));
    commonFuncDeclComp.InitFuncSignature(move(funcRet), bStatic ? (RThisKind)RThisKind_Static {} : RThisKind_Ref{_struct->GetOpenType()}, move(funcParameters), bLastParameterVariadic);
}

RDeclKey& RStructFuncDecl::GetDeclKey()
{
    assert(o_key);
    return *o_key;
}

// from RDecl
RDecl* RStructFuncDecl::GetOuter()
{
    return _struct;
}

RName* RStructFuncDecl::TryGetName()
{
    return &name;
}

size_t RStructFuncDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RStructFuncDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RStructFuncDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RStructFuncDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RStructFuncDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron