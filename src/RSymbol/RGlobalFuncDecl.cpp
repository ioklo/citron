#include "RGlobalFuncDecl.h"
#include "Infra/Exceptions.h"
#include "RNamespaceDecl.h"

namespace Citron {

using namespace std;

RGlobalFuncDecl::RGlobalFuncDecl(RNamespaceDecl* outer, RNamespaceMemberAccessor accessor, TakeRef<RName> name, bool bSeqFunc)
    : outer{outer}
    , accessor{accessor}
    , name{name.Take()}
    , commonFuncDeclComp{bSeqFunc}
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
    return RIdentifier{name, genericsComp.GetTypeParamCount(), commonFuncDeclComp.GetParamIds()};
}

size_t RGlobalFuncDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParamDecl* RGlobalFuncDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeDecl* RGlobalFuncDecl::GetTypeMember(InRef<RName> name, size_t typeParamCount) { return genericsComp.GetTypeMember(name, typeParamCount); }
optional<RDeclRes> RGlobalFuncDecl::GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    // 람다는 검색시키지 않는다
    // 현재 함수에서 Declaration을 할 수 없기 때문에 
    return nullopt;
}

optional<RDeclRes> RGlobalFuncDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = genericsComp.ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (auto o_member = commonFuncDeclComp.ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return outer->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

} // namespace Citron