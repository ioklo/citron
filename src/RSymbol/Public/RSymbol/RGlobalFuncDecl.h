#pragma once
#include "RSymbolConfig.h"

#include "Infra/Ref.h"
#include "RFuncParameter.h"
#include "RCommonFuncDeclComponent.h"
#include "RGenericsComponent.h"
#include "RDecl.h"
#include "ImplRFuncDeclUsingCommonComponents.h"
#include "RDeclKey.h"

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
    std::optional<RDeclKey> o_key;
    RNamespace* outer;
    RNamespaceMemberAccessor accessor;
    RName name;

    RCommonFuncDeclComponent commonFuncDeclComp;
    RGenericsComponent genericsComp;

public:
    RSYMBOL_API RGlobalFuncDecl(RNamespace* outer, RNamespaceMemberAccessor accessor, TakeRef<RName> name, bool bSeqFunc);
    RSYMBOL_API void Init(RDeclKey&& key, std::vector<RTypeParam*>&& typeParams, RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    bool IsSeqFunc() { return commonFuncDeclComp.IsSeqFunc(); }

public: // from RDecl
    RSYMBOL_API RDeclKey& GetDeclKey() final;
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RName* TryGetName() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;

};

} // namespace Citron