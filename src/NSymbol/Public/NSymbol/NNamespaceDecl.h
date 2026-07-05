#pragma once
#include "NSymbolConfig.h"

#include <string>
#include <memory>
#include <optional>

#include "RSymbol/RNamespaceDecl.h"

#include "NNamespaceDeclContainerComponent.h"
#include "NTypeDeclContainerComponent.h"
#include "NFuncDeclContainerComponent.h"
#include "NGlobalFuncDecl.h"

namespace Citron
{
class RNamespaceDeclGroup;

using RFactoryPtr = std::shared_ptr<class RFactory>;

class NNamespaceDecl
    : public RNamespaceDecl
    , private NTypeDeclContainerComponent
    , private NFuncDeclContainerComponent<NGlobalFuncDecl>
{
public:
    using RDeclType = RNamespaceDecl;
    using RDeclResType = RDeclRes_Namespace;

private:
    NNamespaceDecl* outer;
    std::string name;
    RNamespaceDeclGroup* group;
    RFactoryPtr rFactory;

private:
    friend class NFactory;
    NNamespaceDecl(NNamespaceDecl* outer, const std::string& name, RNamespaceDeclGroup* group, const RFactoryPtr& rFactory);

public:
    /*const std::string& GetName() { return name; }

    using NNamespaceDeclContainerComponent::AddNamespace;
    using NNamespaceDeclContainerComponent::GetNamespace;*/

    using NTypeDeclContainerComponent::AddType;
    void AddGlobalFuncDecl(NGlobalFuncDecl* func) { NFuncDeclContainerComponent<NGlobalFuncDecl>::AddFunc(func); }

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDeclOuter
    NDecl* GetNDecl() override { return this; }
    void Accept(NTypeDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RTypeDeclOuter
    // using RNamespaceDecl::Accept;
    // RDecl* GetRDecl() override { return this; }
};

}
