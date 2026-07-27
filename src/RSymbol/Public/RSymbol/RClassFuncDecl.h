#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "ImplRFuncDeclUsingCommonComponents.h"
#include "RFuncDecl.h"
#include "RGenericsComponent.h"
#include "RCommonFuncDeclComponent.h"
#include "RDeclKey.h"

namespace Citron {

class RType;
class RFactory;

class RClassFuncDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents<RClassFuncDecl>
{
    std::optional<RDeclKey> o_key;
    RClassDecl* _class;
    RClassMemberAccessor accessor;
    RName name;

    RGenericsComponent genericsComp;
    RCommonFuncDeclComponent commonFuncDeclComp;

public:
    RSYMBOL_API RClassFuncDecl(RClassDecl* _class, RClassMemberAccessor accessor, bool bSeqFunc, TakeRef<RName> name);    
    void InitTypeParams(std::vector<RTypeParam*>&& typeParams) { genericsComp.InitTypeParams(std::move(typeParams)); }
    RSYMBOL_API void Init(RDeclKey&& key, bool bStatic, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    bool IsSeqFunc() { return commonFuncDeclComp.IsSeqFunc(); }
    
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
