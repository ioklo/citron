#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "ImplRFuncDeclUsingCommonComponents.h"
#include "RFuncDecl.h"
#include "RGenericsComponent.h"
#include "RCommonFuncDeclComponent.h"

namespace Citron {

class RType;
class RFactory;

class RClassFuncDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents<RClassFuncDecl>
{
    RClassDecl* _class;
    RClassMemberAccessor accessor;
    RName name;

    RGenericsComponent genericsComp;
    RCommonFuncDeclComponent commonFuncDeclComp;

public:
    RSYMBOL_API RClassFuncDecl(RClassDecl* _class, RClassMemberAccessor accessor, bool bSeqFunc, TakeRef<RName> name);    
    void InitTypeParams(std::vector<RTypeParamDecl*>&& typeParams) { genericsComp.InitTypeParams(std::move(typeParams)); }
    RSYMBOL_API void InitFuncReturnAndParams(bool bStatic, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    bool IsSeqFunc() { return commonFuncDeclComp.IsSeqFunc(); }
    
public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron
