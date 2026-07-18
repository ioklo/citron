#include "RGlobalFuncDecl.h"
#include "Infra/Exceptions.h"
#include "RNamespaceDecl.h"

namespace Citron {

using namespace std;

RGlobalFuncDecl::RGlobalFuncDecl(RNamespaceDecl* outer, RNamespaceMemberAccessor accessor, TakeRef<RName> name, bool bSeqFunc)
    : outer{outer}
    , accessor{accessor}
    , name{name.Take()}
    , genericsComp{}
    , commonFuncDeclComp{bSeqFunc}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{   
}

void RGlobalFuncDecl::InitFuncReturnAndParams(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    commonFuncDeclComp.InitFuncReturnAndParams(move(funcRet), RThisKind_Static{}, move(funcParameters), bLastParameterVariadic);
}



// from RDecl
RDecl* RGlobalFuncDecl::GetOuter() { return outer; }
RIdentifier RGlobalFuncDecl::GetIdentifier()
{
    return RIdentifier{name, commonFuncDeclComp.GetParamIds()};
}

size_t RGlobalFuncDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RGlobalFuncDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RGlobalFuncDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RGlobalFuncDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RGlobalFuncDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron