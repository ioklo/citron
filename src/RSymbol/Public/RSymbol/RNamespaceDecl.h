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

namespace Citron {

class RNamespaceDeclGroup;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class NNamespaceDecl;
class ENamespaceDecl;

class RNamespaceDecl : public RDecl
{
    RNamespaceDecl* outer;
    RName name;
    RNamespaceDeclGroup* group;
    std::variant<NNamespaceDecl*, ENamespaceDecl*> info;

    RNamespaceDeclContainerComponent namespaceDeclContainerComp;
    RTypeDeclContainerComponent typeDeclContainerComp;
    RFuncDeclContainerComponent<RGlobalFuncDecl> funcDeclContainerComp;
    RFactoryPtr rFactory;
    
public:
    RNamespaceDecl(RNamespaceDecl* outer, RName&& name, RNamespaceDeclGroup* group, const RFactoryPtr& rFactory)
        : outer{outer}, name{std::move(name)}, group{group}, rFactory{rFactory} 
    {}
    void InitInfo(NNamespaceDecl* decl) { info = decl; }

    RName& GetName() { return name; }
    
    void AddNamespace(RNamespaceDecl* _namespace) { namespaceDeclContainerComp.AddNamespace(_namespace); }
    RNamespaceDecl* GetNamespace(InRef<RName> name) { return namespaceDeclContainerComp.GetNamespace(name); }

    void AddType(RTypeDecl* typeDecl) { typeDeclContainerComp.AddType(typeDecl); }
    void AddGlobalFuncDecl(RGlobalFuncDecl* func) { funcDeclContainerComp.AddFunc(func); }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) override;
    RSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron
