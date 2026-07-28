#pragma once
#include "RSymbolConfig.h"
#include <optional>
#include <vector>
#include <memory>
#include <unordered_map>
#include "RDecl.h"
#include "RTypeDecl.h"
#include "RDeclKey.h"

namespace Citron {

class REnumDecl;
struct RFuncParameter;
class RTypeArguments;
class REnumElemVarDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class REnumElemDecl final : public RDecl, public RTypeDecl
{
    RDeclKey key;
    REnumDecl* _enum;
    RName name;
    std::vector<REnumElemVarDecl*> vars; // lazy
    std::unordered_map<RName, REnumElemVarDecl*> varsMap;
    RFactoryPtr rFactory;

public:
    RSYMBOL_API REnumElemDecl(RDeclKey&& key, REnumDecl* _enum, RName&& name, TakeRef<RFactoryPtr> rFactory);
    RSYMBOL_API void AddVar(REnumElemVarDecl* var);

public:
    REnumDecl* GetEnum() { return _enum; }
    RName& GetName() { return name; }
    size_t GetVarCount() { return vars.size(); }
    REnumElemVarDecl* GetUnboundVar(size_t index) { return vars[index]; }
    RSYMBOL_API REnumElemVarDecl* GetUnboundVar(InRef<RName> name);
    bool IsStandalone() { return vars.empty(); }

public: // from RDecl
    RSYMBOL_API RDeclKey& GetDeclKey() final;
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RName* TryGetName() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;

public: // from RTypeDecl
    RSYMBOL_API RDecl* RTypeDecl_GetDecl() final;
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
};

} // namespace Citron
