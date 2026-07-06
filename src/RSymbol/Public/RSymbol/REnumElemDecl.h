#pragma once
#include "RSymbolConfig.h"
#include <optional>
#include <vector>
#include <unordered_map>
#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class REnumDecl;
struct RFuncParameter;
class RTypeArguments;
class REnumElemVarDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class REnumElemDecl final : public RDecl, public RTypeDecl
{
    REnumDecl* _enum;
    RName name;
    std::vector<REnumElemVarDecl*> vars; // lazy
    std::unordered_map<RName, REnumElemVarDecl*> varsMap;
    RFactoryPtr rFactory;

public:
    RSYMBOL_API REnumElemDecl(REnumDecl* _enum, TakeRef<RName> name, TakeRef<RFactoryPtr> rFactory);
    RSYMBOL_API void AddVar(REnumElemVarDecl* var);

public:
    REnumDecl* GetEnum() { return _enum; }
    RName& GetName() { return name; }
    std::optional<RDeclRes_EnumElemVar> ResolveVar(RTypeArguments* typeArgs, InRef<RName> name);
    size_t GetVarCount() { return vars.size(); }
    REnumElemVarDecl* GetUnboundVar(size_t index) { return vars[index]; }
    bool IsStandalone() { return vars.empty(); }

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

} // namespace Citron
