#pragma once
#include "RSymbolConfig.h"
#include <vector>
#include "RDecl.h"
#include "RDeclKey.h"
#include "RAppliedDecl.h"

namespace Citron {

class RTraitDecl;

// 실제 Type이 아니라 requirement
class RTraitTypeDecl : public RDecl
{   
    RTraitDecl* traitDecl;
    RDeclKey key;
    RName name;
    std::vector<RAppliedDecl<RTraitDecl>> traits;

public:
    RSYMBOL_API RTraitTypeDecl(RTraitDecl* trait, RDeclKey&& key, RName&& name);
    RSYMBOL_API void Init(std::vector<RAppliedDecl<RTraitDecl>>&& traits);

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
