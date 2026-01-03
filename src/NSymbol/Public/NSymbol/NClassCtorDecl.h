#pragma once

#include "NSymbolConfig.h"
#include <vector>
#include <string>
#include <optional>

#include "RSymbol/RClassCtorDecl.h"

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "NGenericsComponent.h"
#include "NCommonFuncDeclComponent.h"

namespace Citron
{

class NClassCtorDecl
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RClassCtorDecl
    , private NGenericsComponent
    , private NCommonFuncDeclComponent
{
public:
    NClassDecl* _class;
    RAccessor accessor;
    bool bTrivial;

public:
    NSYMBOL_API NClassCtorDecl(NClassDecl* _class, RAccessor accessor, bool bTrivial);
    NSYMBOL_API void Init(std::vector<RFuncParameter>&& parameters, bool bLastParamVariadic);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDecl
    NDecl* GetNDecl() override { return this; }
    NSYMBOL_API NFuncDeclOuter* GetNFuncDeclOuter() override;
    RFuncReturn GetUnboundFuncReturn() override { return RFuncReturn_ForCtor(); }
    bool IsSeqFunc() override { return NCommonFuncDeclComponent::IsSeqFunc(); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    RTypeParamDecl* GetTypeParam(size_t index) override { return NGenericsComponent::GetTypeParam(index); }
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RFuncDecl
    // NSYMBOL_API RFuncReturn GetReturn(RTypeArguments& typeArgs) override;

    // from RClassCtorDecl
    NSYMBOL_API RClassDecl* GetClassDecl() override;
};

}