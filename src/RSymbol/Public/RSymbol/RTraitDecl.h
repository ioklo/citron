#pragma once
#include "RSymbolConfig.h"
#include <memory>
#include "Infra/Ref.h"
#include "RDecl.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"
#include "RTraitMemberDecl.h"
#include "RNames.h"
#include "RGenericsComponent.h"
#include "RDeclKey.h"

namespace Citron {

using RFactoryPtr = std::shared_ptr<RFactory>;

// public trait MyTrait { void Func(); }
class RTraitDecl : public RDecl, public RTypeDecl
{
    RDeclKey key;
    RTypeDeclOuter outer;
    RName name;
    std::vector<RTraitMemberDecl> members;

    RGenericsComponent genericsComp;
    RFactoryPtr rFactory;

public:
    RSYMBOL_API RTraitDecl(RDeclKey&& key, RTypeDeclOuter outer, RName&& name, TakeRef<RFactoryPtr> rFactory);
    RSYMBOL_API void InitTypeParams(std::vector<RTypeParam*>&& typeParams);
    RSYMBOL_API void AddMember(RTraitMemberDecl&& decl);

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