#pragma once
#include <optional>
#include "RDecl.h"
#include "ImplRFuncDeclUsingCommonComponents.h"
#include "RGenericsComponent.h"
#include "RCommonFuncDeclComponent.h"
#include "RDeclKey.h"

namespace Citron {

class RType;
class RFactory;
class RStructDecl;

class RStructFuncDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents<RStructFuncDecl>
{
    std::optional<RDeclKey> o_key;
    RStructDecl* _struct;
    RStructMemberAccessor accessor;

    RName name;
    RGenericsComponent genericsComp;
    RCommonFuncDeclComponent commonFuncDeclComp;

public:
    RSYMBOL_API RStructFuncDecl(RStructDecl* _struct, RStructMemberAccessor accessor, RName&& name, bool bSeqFunc);
    RSYMBOL_API void Init(RDeclKey&& key, bool bStatic, RFuncReturn&& funcRet, std::vector<RTypeParam*>&& typeParams, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
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
