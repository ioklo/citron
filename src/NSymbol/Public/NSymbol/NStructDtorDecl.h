#pragma once
#include "NSymbolConfig.h"

#include <cassert>
#include <memory>

#include "RSymbol/RStructDtorDecl.h"

#include "NFuncDeclImpl_UsingNCommonFuncDeclComponent.h"

namespace Citron {

class NStructDecl;

class NStructDtorDecl
    : public NFuncDeclImpl_UsingNCommonFuncDeclComponent<RStructDtorDecl>
{
public:
    RAccessor accessor;
    NStructDecl* _struct;

public:
    NSYMBOL_API NStructDtorDecl(RAccessor accessor, NStructDecl* structDecl);
    using NCommonFuncDeclComponent::IsSeqFunc;

    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    RIdentifier GetIdentifier() override { return RIdentifier{RName_Reserved("Dtor"), 0, {}}; }
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
};



} // namespace Citron
