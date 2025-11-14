#pragma once
#include "NSymbolConfig.h"

#include <optional>
#include <vector>
#include <unordered_map>

#include "RSymbol/REnumElemDecl.h"

#include "NDecl.h"
#include "NTypeDecl.h"
#include "NEnumElemVarDecl.h"

namespace Citron
{

class NEnumElemDecl
    : public NDecl
    , public NTypeDecl
    , public REnumElemDecl
{
public:
    NEnumDecl* _enum;
    std::string name;
    std::vector<NEnumElemVarDecl*> vars; // lazy
    std::unordered_map<std::string, NEnumElemVarDecl*> varsMap;

public:
    NSYMBOL_API NEnumElemDecl(NEnumDecl* _enum);
    NSYMBOL_API void Init(const std::string& name);
    NSYMBOL_API void AddVar(NEnumElemVarDecl* var);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RMember ToRMember(RTypeArguments* typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory) override;

    // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }

    // from REnumElemDecl    
    NSYMBOL_API REnumDecl* GetBaseEnumDecl() override;
    NSYMBOL_API std::optional<RMember_EnumElemVar> GetVar(RTypeArguments* typeArgs, const RName& name) override;
    NSYMBOL_API size_t GetVarCount() override;
    bool IsStandalone() override { return vars.empty(); }
    NSYMBOL_API std::vector<RFuncParameter> GetUnboundCtorParams() override;
};

}