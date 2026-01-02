#pragma once
#include "NSymbolConfig.h"
#include "RSymbol/RTypeParamDecl.h"
#include "RSymbol/RNames.h"

#include "NDecl.h"
#include "NTypeDecl.h"

namespace Citron {

// N'TypeParam'Decl
class NTypeParamDecl 
    : public NDecl
    , public NTypeDecl
    , public RTypeParamDecl
{
    // NTypeParamDeclOuter를 만들지, 그냥 NDecl로 할지. 일단 쓰이는데가 있을때까지는 NDecl로 한다
    NDecl* outer;
    RName name;
    size_t globalIndex;

public:
    NSYMBOL_API NTypeParamDecl(NDecl* outer, RName&& name, size_t globalIndex);
    NSYMBOL_API ~NTypeParamDecl();

public: // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override { return outer; }
    void Accept(NDeclVisitor& visitor) { visitor.Visit(this); }

public: // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RTypeDecl* GetRTypeDecl() { return this; }
    NSYMBOL_API RMember ToRMember(RTypeArguments* typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(this); }

public: // from RTypeParamDecl
    size_t GetGlobalIndex() override { return globalIndex; }

public: // from RDecl
    RDecl* GetROuter() override { return this; }
    RAccessor GetAccessor() override { return RAccessor::Public; }
    RIdentifier GetIdentifier() override { return RIdentifier{name, 0, {}}; }
    size_t GetTypeParamCount() override { return 0; }
    RTypeParamDecl* GetTypeParam(size_t index) override { return nullptr; }
    RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override { return nullptr; }
    std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override { return std::nullopt; }
    std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override { return std::nullopt; }

public: // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }
};


} // namespce Citron
