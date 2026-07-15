#pragma once
#include <variant>
#include <memory>
#include "Infra/Ref.h"
#include "RNamespaceDeclContainerComponent.h"
#include "RTypeDeclContainerComponent.h"
#include "RFuncDeclContainerComponent.h"
#include "RNames.h"
#include "RDecl.h"
#include "RGlobalFuncDecl.h"
#include "RDeclRes.h"

namespace Citron {

class RNamespaceDeclGroup;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class RNamespaceDecl final : public RDecl
{
    RNamespaceDecl* outer;
    RName name;
    RNamespaceDeclGroup* group;
    
    RNamespaceDeclContainerComponent namespaceDeclContainerComp;
    RTypeDeclContainerComponent typeDeclContainerComp;
    RFuncDeclContainerComponent<RGlobalFuncDecl, RDeclRes_GlobalFuncs> funcDeclContainerComp;
    RFactoryPtr rFactory;
    
public:
    RNamespaceDecl(RNamespaceDecl* outer, RName&& name, RNamespaceDeclGroup* group, TakeRef<RFactoryPtr> rFactory)
        : outer{outer}, name{std::move(name)}, group{group}, rFactory{rFactory.Take()} 
    {}

    RNamespaceDecl* GetOuterNamespace() { return outer; }
    RName& GetName() { return name; }
    
    void AddNamespace(RNamespaceDecl* _namespace) { namespaceDeclContainerComp.AddNamespace(_namespace); }
    RNamespaceDecl* GetNamespace(InRef<RName> name) { return namespaceDeclContainerComp.GetNamespace(name); }

    void AddType(RTypeDecl* typeDecl) { typeDeclContainerComp.AddType(typeDecl); }
    void AddGlobalFuncDecl(RGlobalFuncDecl* func) { funcDeclContainerComp.AddFunc(func); }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron
