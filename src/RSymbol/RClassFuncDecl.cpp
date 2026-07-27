#include "RClassFuncDecl.h"
#include "RClassDecl.h"
#include "RThisKind.h"

using namespace std;

namespace Citron {

RClassFuncDecl::RClassFuncDecl(RClassDecl* _class, RClassMemberAccessor accessor, bool bSeqFunc, TakeRef<RName> name)
    : _class{_class}
    , accessor{accessor}
    , name{name.Take()}
    , genericsComp{}
    , commonFuncDeclComp{bSeqFunc}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
}

void RClassFuncDecl::Init(RDeclKey&& key, bool bStatic, RFuncReturn&& funcReturn, vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    o_key.emplace(std::move(key));

    commonFuncDeclComp.InitFuncSignature(
        move(funcReturn),
        bStatic ? (RThisKind)RThisKind_Static {} : RThisKind_Handle{_class->GetOpenType()},
        move(funcParameters),
        bLastParameterVariadic);
}

// from RDecl

RDeclKey& RClassFuncDecl::GetDeclKey()
{
    assert(o_key);
    return *o_key;
}

RDecl* RClassFuncDecl::GetOuter()
{
    return _class;
}

RName* RClassFuncDecl::TryGetName()
{
    return &name;
}

size_t RClassFuncDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RClassFuncDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RClassFuncDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RClassFuncDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RClassFuncDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron