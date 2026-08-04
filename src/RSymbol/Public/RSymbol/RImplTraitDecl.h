#pragma once
#include "RSymbolConfig.h"
#include <vector>
#include "RDecl.h"
#include "RGenericsComponent.h"
#include "RDeclKey.h"
#include "RAppliedDecl.h"

namespace Citron {

class RTraitDecl;
class RTypeArguments;
class RImplTraitMemberDecl;
class RTypeParam;

// RImplTrait의 outer는 무엇인가? impl할 대상을 따라가게 된다
class RImplTraitDecl final : public RDecl
{
    struct LazyInit
    {
        RDeclKey key;
        RAppliedDecl<RTraitDecl> appliedTraitDecl;
    };


    RDecl* target; // outer는 target의 outer를 리턴하면 된다. 일단 struct, extension인데, 특징적인 뭔가가 있는 경우 RImplTraitDeclOuter를 만들자
    std::optional<LazyInit> o_lazyInit;
    std::vector<RImplTraitMemberDecl> members;
    RGenericsComponent genericsComp;

public:
    RSYMBOL_API RImplTraitDecl(RDecl* target);
    RSYMBOL_API void Init(RDeclKey&& key, std::vector<RTypeParam*>&& typeParams, RAppliedDecl<RTraitDecl> appliedTraitDecl);
    RSYMBOL_API void AddMember(RImplTraitMemberDecl&& decl);
    RDecl* GetTarget() { return target; }

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
