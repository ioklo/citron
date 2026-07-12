#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "ImplRFuncDeclUsingCommonComponents.h"
#include "RGenericsComponent.h"
#include "RCommonFuncDeclComponent.h"

namespace Citron {

class RClassDecl;
enum class RClassMemberAccessor;

class RClassCtorDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents<RClassCtorDecl>
{
    RClassDecl* _class;
    RClassMemberAccessor accessor;
    bool bTrivial;

    RGenericsComponent genericsComp;
    RCommonFuncDeclComponent commonFuncDeclComp;

public:
    RSYMBOL_API RClassCtorDecl(RClassDecl* _class, RClassMemberAccessor accessor, bool bTrivial);
    RSYMBOL_API void InitTypeParams(std::vector<RTypeParamDecl*>&& typeParams);
    RClassDecl* GetClassDecl() { return _class; }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron
