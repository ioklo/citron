#include "RImplTraitFuncDecl.h"
#include "Infra/Exceptions.h"
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

void RImplTraitFuncDecl::Init(RDeclKey&& key, vector<RTypeParam*>&& typeParams, RFuncReturn&& funcRet, vector<RFuncParameter>&& funcParams)
{
    o_key.emplace(move(key));
    genericsComp.InitTypeParams(move(typeParams));
    // commonFuncDeclComp.InitFuncSignature(move(funcRet), move(funcParams));

    // TODO: [66] 2026-07-09, Trait, Extend 구현
    throw NotImplementedException{};
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