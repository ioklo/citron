#pragma once
#include "RDecl.h"
#include "ImplRFuncDeclUsingCommonComponents.h"
#include "RGenericsComponent.h"
#include "RCommonFuncDeclComponent.h"

namespace Citron {

class RType;
class RFactory;

class RStructFuncDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents
{
    RStructDecl* _struct;
    RStructMemberAccessor accessor;

    RName name;
    RGenericsComponent genericsComp;
    RCommonFuncDeclComponent commonFuncDeclComp;

public:
    RSYMBOL_API RStructFuncDecl(RStructDecl* _struct, RStructMemberAccessor accessor, TakeRef<RName> name, bool bSeqFunc);
    void InitTypeParams(std::vector<RTypeParamDecl*>&& typeParams) { genericsComp.InitTypeParams(std::move(typeParams)); }
    RSYMBOL_API void InitFuncReturnAndParams(bool bStatic, RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    bool IsSeqFunc() { return commonFuncDeclComp.IsSeqFunc(); }
    
public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) override;
    RSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron
