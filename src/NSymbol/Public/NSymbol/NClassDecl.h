#pragma once
#include "NSymbolConfig.h"

#include <memory>

#include "RSymbol/RClassDecl.h"

#include "NDecl.h"
#include "NTypeDecl.h"
#include "NTypeDeclOuter.h"
#include "NClassCtorDecl.h"
#include "NClassFuncDecl.h"
#include "NClassVarDecl.h"
#include "NGenericsComponent.h"
#include "NTypeDeclContainerComponent.h"
#include "NFuncDeclContainerComponent.h"
#include "NTypeDeclOuter.h"

namespace Citron
{

using RFactoryPtr = std::shared_ptr<class RFactory>;

class NTypeParamDecl;

class NClassDecl
    : public NDecl
    , public NTypeDecl
    , public NTypeDeclOuter
    , public NFuncDeclOuter
    , public RClassDecl
    , private NGenericsComponent
    , private NTypeDeclContainerComponent
    , private NFuncDeclContainerComponent<NClassFuncDecl>
{
    struct BaseTypes
    {
        RType_Class* baseClass;
        std::vector<RType_Interface> interfaces;
    };

    NTypeDeclOuter* outer;
    RAccessor accessor;

    RName name;
    std::vector<NClassCtorDecl*> ctors;
    int trivialCtorIndex; // can be -1

    std::vector<NClassVarDecl*> vars;
    std::optional<BaseTypes> oBaseTypes;
    std::unordered_map<RName, NClassVarDecl*> varsMap;

    RFactoryPtr rFactory;

public:
    using NTypeDeclContainerComponent::AddType;

    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RTypeDecl* GetRTypeDecl() override { return this; }
    NSYMBOL_API RMember ToRMember(RTypeArguments* typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NTypeDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RDecl* GetROuter() override;
    NSYMBOL_API RIdentifier GetIdentifier() override;
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    RTypeParamDecl* GetTypeParam(size_t index) override { return NGenericsComponent::GetTypeParam(index); }
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RFuncDeclOuter
    //RDecl* GetRDecl() override { return this; }

    // from RClassDecl
    NSYMBOL_API std::optional<RMember_ClassVar> GetVar(RTypeArguments* typeArgs, const RName& name) override;
};

}