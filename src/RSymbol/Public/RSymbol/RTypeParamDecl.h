#pragma once
#include "RSymbolConfig.h"
#include "RDecl.h"
#include "RTypeDecl.h"
#include "RNames.h"

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;

class RTypeParamDecl final : public RDecl, public RTypeDecl
{
    // RTypeParamDeclOuter를 만들지, 그냥 RDecl로 할지. 일단 쓰이는데가 있을때까지는 RDecl로 한다
    RDecl* outer;
    RName name;
    size_t globalIndex;
    RFactoryPtr rFactory;

public:
    RSYMBOL_API RTypeParamDecl(RDecl* outer, RName&& name, size_t globalIndex, TakeRef<RFactoryPtr> rFactory);
    RName& GetName() { return name; }
    size_t GetGlobalIndex() { return globalIndex; }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;

public: // from RTypeDecl
    RSYMBOL_API RDecl* RTypeDecl_GetDecl() override;
    RSYMBOL_API RType* GetOpenType() override;
    RSYMBOL_API RDeclRes ToRDeclRes(RTypeArguments* typeArgs) override;
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) override;
};


} // namespace Citron
