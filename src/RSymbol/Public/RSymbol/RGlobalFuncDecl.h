#pragma once
#include "RSymbolConfig.h"

#include "Infra/Ref.h"
#include "RFuncParameter.h"
#include "RCommonFuncDeclComponent.h"
#include "RGenericsComponent.h"
#include "RDecl.h"
#include "ImplRFuncDeclUsingCommonComponents.h"

namespace Citron {

class EGlobalFuncDecl;

class RType;
class RFactory;
class RTypeParamDecl;
class RTypeArguments;
class RFuncReturn;
enum class RNamespaceMemberAccessor;

class RGlobalFuncDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents
{
    RNamespaceDecl* outer;
    RNamespaceMemberAccessor accessor;
    RName name;

    RCommonFuncDeclComponent commonFuncDeclComp;
    RGenericsComponent genericsComp;

public:
    RSYMBOL_API RGlobalFuncDecl(RNamespaceDecl* outer, RNamespaceMemberAccessor accessor, TakeRef<RName> name, bool bSeqFunc);
    void InitTypeParams(std::vector<RTypeParamDecl*>&& typeParams) { genericsComp.InitTypeParams(std::move(typeParams)); }
    RSYMBOL_API void InitFuncReturnAndParams(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    bool IsSeqFunc() { return commonFuncDeclComp.IsSeqFunc(); }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RIdentifier GetIdentifier() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) final;
    RSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) final;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) final;
};

} // namespace Citron