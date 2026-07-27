#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "ImplRFuncDeclUsingCommonComponents.h"
#include "RGenericsComponent.h"
#include "RCommonFuncDeclComponent.h"
#include "RDeclKey.h"

namespace Citron {

class RClassDecl;
enum class RClassMemberAccessor;

class RClassCtorDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents<RClassCtorDecl>
{
    RDeclKey key;
    RClassDecl* _class;
    RClassMemberAccessor accessor;
    bool bTrivial;

    RGenericsComponent genericsComp;
    RCommonFuncDeclComponent commonFuncDeclComp;

public:
    RSYMBOL_API RClassCtorDecl(RDeclKey&& key, RClassDecl* _class, RClassMemberAccessor accessor, bool bTrivial);
    RSYMBOL_API void InitTypeParams(std::vector<RTypeParam*>&& typeParams);
    RClassDecl* GetClassDecl() { return _class; }

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
