#pragma once
#include "RSymbolConfig.h"
#include <variant>

#include "RDeclKey.h"
#include "RTypeDeclOuter.h"
#include "RDecl.h"
#include "RTypeDecl.h"
#include "RAppliedDecl.h"
#include "RGenericsComponent.h"

namespace Citron {

struct RTypeAliasTarget_Struct { RAppliedDecl<RStructDecl> appliedStruct; };
struct RTypeAliasTarget_Class { RAppliedDecl<RClassDecl> appliedClass; };
struct RTypeAliasTarget_Enum { RAppliedDecl<REnumDecl> appliedEnum; };
struct RTypeAliasTarget_Interface { RAppliedDecl<RInterfaceDecl> appliedInterface; };

// RTypeAlias를 제외한 최종 형태
using RTypeAliasTarget = std::variant<RTypeAliasTarget_Struct, RTypeAliasTarget_Class, RTypeAliasTarget_Enum, RTypeAliasTarget_Interface>;

// class C2<T1> { type T<T2> = C1<T1, T2>; }
class RTypeAliasDecl final : public RDecl, public RTypeDecl
{   
    RDeclKey key;
    RName name;
    RTypeDeclOuter outer;
    RTypeAliasTarget target;
    RGenericsComponent genericsComponent;

public:
    RSYMBOL_API RTypeAliasDecl(RDeclKey&& key, RTypeDeclOuter&& outer, RName&& name);
    RSYMBOL_API void Init(std::vector<RTypeParam*>&& typeParams, RTypeAliasTarget&& target);

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