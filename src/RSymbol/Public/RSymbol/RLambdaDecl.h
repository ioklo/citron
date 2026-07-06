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
class RLambdaDecl final : public RDecl, public RTypeDecl, public ImplRFuncDeclUsingCommonComponents
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
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) override;
    RSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;

public: // from RTypeDecl
    RSYMBOL_API RDecl* RTypeDecl_GetDecl() override;
    RSYMBOL_API RType* GetOpenType() override;
    RSYMBOL_API RDeclRes ToRDeclRes(RTypeArguments* typeArgs) override;
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) override;
};

// M버전이 없다

} // namespace Citron
