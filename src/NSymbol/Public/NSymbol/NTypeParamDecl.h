#pragma once
#include "NSymbolConfig.h"
#include "RSymbol/RTypeParamDecl.h"
#include "RSymbol/RNames.h"

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;

// N'TypeParam'Decl
class NTypeParamDecl 
    : public RTypeParamDecl
{
    // NTypeParamDeclOuter를 만들지, 그냥 NDecl로 할지. 일단 쓰이는데가 있을때까지는 NDecl로 한다
    NDecl* outer;
    RName name;
    size_t globalIndex;
    RFactoryPtr rFactory;

public:
    NSYMBOL_API NTypeParamDecl(NDecl* outer, RName&& name, size_t globalIndex, const RFactoryPtr& rFactory);
    NSYMBOL_API ~NTypeParamDecl();

    const RName& GetName() { return name; }

public: // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override { return outer; }
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

public: // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RTypeDecl* GetRTypeDecl() override { return this; }
    NSYMBOL_API RDeclRes ToRDeclRes(RTypeArguments* typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(this); }

public: // from RTypeParamDecl
    size_t GetGlobalIndex() override { return globalIndex; }

public: // from RDecl
    RDecl* GetROuter() override { return this; }
    RAccessor GetAccessor() override { return RAccessor::Public; }
    RIdentifier GetIdentifier() override { return RIdentifier{name, 0, {}}; }
    RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override { return nullptr; }
    std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override { return std::nullopt; }
    std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override { return std::nullopt; }

public: // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }
    RType* GetOpenType() override;
};


} // namespce Citron
