#pragma once

#include "NSymbolConfig.h"

#include <vector>
#include <optional>
#include <unordered_map>

#include "RSymbol/REnumDecl.h"

#include "NDecl.h"
#include "NTypeDecl.h"
#include "NEnumElemDecl.h"
#include "NTypeDeclOuter.h"
#include "NGenericsComponent.h"

namespace Citron
{

class NTypeParamDecl;

class NEnumDecl
    : public NDecl
    , public NTypeDecl
    , public REnumDecl
    , private NGenericsComponent
{
    NTypeDeclOuter* outer;
    RAccessor accessor;

    RName name;
    std::vector<NEnumElemDecl*> elems;
    std::unordered_map<RName, NEnumElemDecl*> elemsMap;

    // std::unordered_map<std::string, int> elemsByName;

public:
    NSYMBOL_API NEnumDecl(NTypeDeclOuter* outer, RAccessor accessor, const RName& name);
    using NGenericsComponent::InitTypeParams;
    NSYMBOL_API void AddElem(NEnumElemDecl* elem);

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
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    RTypeParamDecl* GetTypeParam(size_t index) override { return NGenericsComponent::GetTypeParam(index); }
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }
};

}

