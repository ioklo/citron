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

namespace Citron {

using RFactoryPtr = std::shared_ptr<RFactory>;

// public trait MyTrait { void Func(); }
class RTraitDecl : public RDecl
{
    RTypeDeclOuter outer;
    RName name;
    std::vector<RTraitMemberDecl> members;

    RGenericsComponent genericsComp;
    RFactoryPtr rFactory;

public:
    RSYMBOL_API RTraitDecl(RTypeDeclOuter outer, RName&& name, TakeRef<RFactoryPtr> rFactory);
    RSYMBOL_API void InitTypeParams(std::vector<RTypeParamDecl*>&& typeParams);
    void AddMember(RTraitMemberDecl&& decl) { members.push_back(std::move(decl)); }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RIdentifier GetIdentifier() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RDeclRes> ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) final;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) final;
};

} // namespace Citron