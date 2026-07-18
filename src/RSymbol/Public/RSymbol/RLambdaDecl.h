#pragma once
#include "RSymbolConfig.h"
#include <optional>
#include <unordered_map>
#include <memory>
#include "Infra/Ref.h"
#include "RDecl.h"
#include "RTypeDecl.h"
#include "RFuncDecl.h"
#include "RGenericsComponent.h"
#include "RCommonFuncDeclComponent.h"
#include "ImplRFuncDeclUsingCommonComponents.h"

namespace Citron {

class RType;
using RFactoryPtr = std::shared_ptr<class RFactory>;

// TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
class RLambdaDecl final : public RDecl, public RTypeDecl, public ImplRFuncDeclUsingCommonComponents<RLambdaDecl>
{
    RFuncDecl* outer;
    RName name;

    // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
    std::optional<std::vector<RLambdaVarDecl*>> vars;
    std::unordered_map<RName, RLambdaVarDecl*> varsMap;

    RGenericsComponent genericsComp;
    RCommonFuncDeclComponent commonFuncDeclComp;

    RFactoryPtr rFactory;

public:
    RSYMBOL_API RLambdaDecl(RFuncDecl* outer, RName&& name, TakeRef<RFactoryPtr> rFactory);
    RSYMBOL_API void InitFuncReturnAndParameters(RFuncReturn&& funcReturn, RThisKind&& thisKind, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    RSYMBOL_API void InitVars(std::vector<RLambdaVarDecl*>&& vars);
    bool IsSeqFunc() { return commonFuncDeclComp.IsSeqFunc(); }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RIdentifier GetIdentifier() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;

public: // from RTypeDecl
    RSYMBOL_API RDecl* RTypeDecl_GetDecl() final;
    RSYMBOL_API RType* GetOpenType() final;
    RSYMBOL_API RTypeRes ToRTypeRes(RTypeArguments* typeArgs) final;
    RSYMBOL_API RDeclRes ToRDeclRes(RTypeArguments* typeArgs) final;
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
};

// M버전이 없다

} // namespace Citron
