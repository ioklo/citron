#pragma once
#include "RDecl.h"

namespace Citron {

class RType;
class RFactory;
class REnumElemDecl;

class REnumElemVarDecl final : public RDecl
{
    REnumElemDecl* enumElem;
    RName name;
    RType* declType; // lazy-init

public:
    RSYMBOL_API REnumElemVarDecl(REnumElemDecl* outer, TakeRef<RName> name);
    void InitDeclType(RType* declType) { this->declType = declType; }
    
    REnumElemDecl* GetEnumElem() { return enumElem; }
    RName& GetName() { return name; }
    RType* GetUnboundDeclType() { return declType; }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RIdentifier GetIdentifier() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;
};


} // namespace Citron
