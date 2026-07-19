#pragma once
#include "RSymbolConfig.h"
#include "Infra/Ref.h"
#include "RDecl.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"
#include "RGenericsComponent.h"
#include "RFactory.h"

namespace Citron {

class RInterfaceDecl final : public RDecl, public RTypeDecl
{
    RTypeDeclOuter outer;
    RName name;
    RGenericsComponent genericsComp;
    RFactoryPtr rFactory;

public:
    RSYMBOL_API RInterfaceDecl(RTypeDeclOuter outer, RName&& name, TakeRef<RFactoryPtr> rFactory);
    void InitTypeParams(std::vector<RTypeParam*>&& typeParams) { genericsComp.InitTypeParams(std::move(typeParams)); }

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
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
};

} // namespace Citron

