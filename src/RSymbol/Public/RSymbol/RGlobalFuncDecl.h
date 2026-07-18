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
class RTypeParam;
class RTypeArguments;
class RFuncReturn;
enum class RNamespaceMemberAccessor;

class RGlobalFuncDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents<RGlobalFuncDecl>
{
    RNamespaceDecl* outer;
    RNamespaceMemberAccessor accessor;
    RName name;

    RCommonFuncDeclComponent commonFuncDeclComp;
    RGenericsComponent genericsComp;

public:
    RSYMBOL_API RGlobalFuncDecl(RNamespaceDecl* outer, RNamespaceMemberAccessor accessor, TakeRef<RName> name, bool bSeqFunc);
    void InitTypeParams(std::vector<RTypeParam*>&& typeParams) { genericsComp.InitTypeParams(std::move(typeParams)); }
    RSYMBOL_API void InitFuncReturnAndParams(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    bool IsSeqFunc() { return commonFuncDeclComp.IsSeqFunc(); }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RIdentifier GetIdentifier() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;

};

} // namespace Citron