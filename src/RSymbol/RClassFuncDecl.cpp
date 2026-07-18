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

void RClassFuncDecl::InitFuncReturnAndParams(bool bStatic, RFuncReturn&& funcReturn, vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    commonFuncDeclComp.InitFuncReturnAndParams(
        move(funcReturn),
        bStatic ? (RThisKind)RThisKind_Static {} : RThisKind_Handle{_class->GetOpenType()},
        move(funcParameters),
        bLastParameterVariadic);
}

// from RDecl

RDecl* RClassFuncDecl::GetOuter()
{
    return _class;
}

RIdentifier RClassFuncDecl::GetIdentifier()
{
    return RIdentifier{name, commonFuncDeclComp.GetParamIds()};
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