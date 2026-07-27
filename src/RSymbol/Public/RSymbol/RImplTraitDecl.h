#pragma once
#include "RSymbolConfig.h"
#include <vector>
#include "RDecl.h"
#include "RGenericsComponent.h"
#include "RDeclKey.h"

namespace Citron {

class RTraitDecl;
class RTypeArguments;
class RImplTraitMemberDecl;
class RTypeParam;

// RImplTrait의 outer는 무엇인가? impl할 대상을 따라가게 된다
class RImplTraitDecl final : public RDecl
{
    RDeclKey key;

    RDecl* target; // outer는 target의 outer를 리턴하면 된다. 일단 struct, extension인데, 특징적인 뭔가가 있는 경우 RImplTraitDeclOuter를 만들자
    RTraitDecl* trait;
    RTypeArguments* typeArgs;
    
    std::vector<RImplTraitMemberDecl> members;

    RGenericsComponent genericsComp;

public:
    RSYMBOL_API RImplTraitDecl(RDeclKey&& key, RDecl* target, RTraitDecl* trait, RTypeArguments* typeArgs);
    RSYMBOL_API void Init(std::vector<RTypeParam*>&& typeParams);
    void AddMember(RImplTraitMemberDecl&& decl);

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
