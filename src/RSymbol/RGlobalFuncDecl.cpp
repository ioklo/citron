#include "RGlobalFuncDecl.h"
#include "Infra/Exceptions.h"
#include "RNamespace.h"
#include "RDeclKey.h"

namespace Citron {

using namespace std;

RGlobalFuncDecl::RGlobalFuncDecl(RNamespace* outer, RNamespaceMemberAccessor accessor, TakeRef<RName> name, bool bSeqFunc)
    : outer{outer}
    , accessor{accessor}
    , name{name.Take()}
    , genericsComp{}
    , commonFuncDeclComp{bSeqFunc}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{   
}

void RGlobalFuncDecl::Init(RDeclKey&& key, std::vector<RTypeParam*>&& typeParams, RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    o_key.emplace(std::move(key));
    genericsComp.InitTypeParams(move(typeParams));
    commonFuncDeclComp.InitFuncSignature(move(funcRet), RThisKind_Static{}, move(funcParameters), bLastParameterVariadic);
}

// from RDecl
RDeclKey& RGlobalFuncDecl::GetDeclKey() { return *o_key; }
RDecl* RGlobalFuncDecl::GetOuter() { return outer; }
RName* RGlobalFuncDecl::TryGetName() {  return &name; }

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