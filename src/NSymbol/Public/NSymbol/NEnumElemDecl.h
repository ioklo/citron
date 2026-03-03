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
    RName name;
    std::vector<NEnumElemVarDecl*> vars; // lazy
    std::unordered_map<RName, NEnumElemVarDecl*> varsMap;

public:
    NSYMBOL_API NEnumElemDecl(NEnumDecl* _enum, const RName& name);
    NSYMBOL_API void AddVar(NEnumElemVarDecl* var);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RTypeDecl* GetRTypeDecl() override { return this; }
    RDeclRes ToRDeclRes(RTypeArguments* typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    size_t GetTypeParamCount() override { return 0; }
    RTypeParamDecl* GetTypeParam(size_t index) override { return nullptr; }
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }

    // from REnumElemDecl    
    NSYMBOL_API REnumDecl* GetBaseEnumDecl() override;
    NSYMBOL_API std::optional<RDeclRes_EnumElemVar> GetVar(RTypeArguments* typeArgs, const RName& name) override;
    NSYMBOL_API REnumElemVarDecl* GetVarDecl(size_t index) override;
    NSYMBOL_API size_t GetVarCount() override;
    bool IsStandalone() override { return vars.empty(); }
};

}