#include "RStructFuncDecl.h"
#include "RStructDecl.h"

using namespace std;

namespace Citron {

RStructFuncDecl::RStructFuncDecl(RStructDecl* _struct, RStructMemberAccessor accessor, TakeRef<RName> name, bool bSeqFunc)
    : _struct{_struct}, accessor{accessor}, name{name.Take()}
    , genericsComp{}
    , commonFuncDeclComp{bSeqFunc}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
}

void RStructFuncDecl::InitFuncReturnAndParams(bool bStatic, RFuncReturn&& funcRet, vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    commonFuncDeclComp.InitFuncReturnAndParams(move(funcRet), bStatic ? (RThisKind)RThisKind_Static {} : RThisKind_Ref{_struct->GetOpenType()}, move(funcParameters), bLastParameterVariadic);
}

// from RDecl
RDecl* RStructFuncDecl::GetOuter()
{
    return _struct;
}

RIdentifier RStructFuncDecl::GetIdentifier()
{
    return RIdentifier{name, commonFuncDeclComp.GetParamIds()};
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