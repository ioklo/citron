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
    RFuncDeclContainerComponent<RGlobalFuncDecl, RMember_GlobalFuncs> funcDeclContainerComp;
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
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RIdentifier GetIdentifier() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;

    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;
};

} // namespace Citron
