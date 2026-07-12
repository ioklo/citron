#pragma once
#include "RSymbolConfig.h"
#include <vector>
#include <unordered_map>
#include "RDecl.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"
#include "RGenericsComponent.h"

namespace Citron {

class REnumElemDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class REnumDecl final : public RDecl, public RTypeDecl
{
    RTypeDeclOuter outer;
    RName name;
    std::vector<REnumElemDecl*> elems;
    std::unordered_map<RName, REnumElemDecl*> elemsMap;
    RFactoryPtr rFactory;

    RGenericsComponent genericsComp;

public:
    RSYMBOL_API REnumDecl(RTypeDeclOuter outer, TakeRef<RName> name, TakeRef<RFactoryPtr> rFactory);
    void InitTypeParams(std::vector<RTypeParamDecl*>&& typeParams) { genericsComp.InitTypeParams(std::move(typeParams)); }
    RSYMBOL_API void AddElem(REnumElemDecl* elem);

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;

public: // from RTypeDecl
    RSYMBOL_API RDecl* RTypeDecl_GetDecl() override;
    RSYMBOL_API RType* GetOpenType() override;
    RSYMBOL_API RDeclRes ToRDeclRes(RTypeArguments* typeArgs) override;
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) override;
};

} // namespace Citron
