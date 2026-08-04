#include "RImplTraitFuncDecl.h"
#include "Infra/Exceptions.h"
#include "RStructDecl.h"
#include "RImplTraitDecl.h"
#include "RMember.h"

using namespace std;

namespace Citron {
RImplTraitFuncDecl::RImplTraitFuncDecl(RImplTraitDecl* implTrait, bool bSeqFunc, RName&& name)
    : implTrait{implTrait}
    , name{move(name)}
    , genericsComp{}
    , commonFuncDeclComp{bSeqFunc}
    , ImplRFuncDeclUsingCommonComponents<RImplTraitFuncDecl>{this, commonFuncDeclComp}
{
}

void RImplTraitFuncDecl::Init(RDeclKey&& key, vector<RTypeParam*>&& typeParams, RFuncReturn&& funcRet, RThisKind&& thisKind, vector<RFuncParameter>&& funcParams, bool bLastParamVariadic)
{
    o_key.emplace(move(key));
    genericsComp.InitTypeParams(move(typeParams));
    commonFuncDeclComp.InitFuncSignature(move(funcRet), move(thisKind), move(funcParams), bLastParamVariadic);
}

RDeclKey& RImplTraitFuncDecl::GetDeclKey()
{
    assert(o_key);
    return *o_key;
}

RDecl* RImplTraitFuncDecl::GetOuter()
{
    return implTrait;
}

RName* RImplTraitFuncDecl::TryGetName()
{
    return &name;
}

size_t RImplTraitFuncDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RImplTraitFuncDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RImplTraitFuncDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RImplTraitFuncDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RImplTraitFuncDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron