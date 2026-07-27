#pragma once
#include <variant>
#include <memory>
#include "Infra/Ref.h"
#include "RNamespaceDeclContainerComponent.h"
#include "RTypeDeclContainerComponent.h"
#include "RFuncDeclContainerComponent.h"
#include "RNames.h"
#include "RDecl.h"
#include "RDeclKey.h"
#include "RGlobalFuncDecl.h"

namespace Citron {

class RModule;
class RNamespaceDeclGroup;
using RFactoryPtr = std::shared_ptr<class RFactory>;

struct RNamespaceKind_Root { RModule* _module; };
struct RNamespaceKind_Normal { RNamespace* outer; RName name; };

class RNamespaceKind
{
    using Variant = std::variant<RNamespaceKind_Root, RNamespaceKind_Normal>;
    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, RNamespaceKind>) && std::constructible_from<Variant, T&&>
    RNamespaceKind(T&& t) : v(std::forward<T>(t)) {}

    auto Visit(auto&& visitor) { return std::visit(std::forward<decltype(visitor)>(visitor), v); }
    RName& GetName() { return std::get<RNamespaceKind_Normal>(v).name; }
};

class RNamespace : public RDecl
{
    RDeclKey key;
    RNamespaceKind kind;
    
    RNamespaceDeclContainerComponent namespaceDeclContainerComp;
    RTypeDeclContainerComponent typeDeclContainerComp;
    RFuncDeclContainerComponent<RGlobalFuncDecl, RMember_GlobalFuncs> funcDeclContainerComp;
    RFactoryPtr rFactory;
    
public:
    RNamespace(RDeclKey&& key, RNamespaceKind&& kind, TakeRef<RFactoryPtr> rFactory)
        : key{std::move(key)}, kind{std::move(kind)}, rFactory{rFactory.Take()} 
    {}

    RName& GetName() { return kind.GetName(); }
    
    void AddNamespace(RNamespace* _namespace) { namespaceDeclContainerComp.AddNamespace(_namespace); }
    RNamespace* GetNamespace(InRef<RName> name) { return namespaceDeclContainerComp.GetNamespace(name); }

    void AddType(RTypeDecl* typeDecl) { typeDeclContainerComp.AddType(typeDecl); }
    void AddGlobalFuncDecl(RGlobalFuncDecl* func) { funcDeclContainerComp.AddFunc(func); }

public: // from RDecl
    RSYMBOL_API RDeclKey& GetDeclKey() final;
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RName* TryGetName() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;

    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;

private:
    void FillIdentifier(std::string& buffer) final;
};

} // namespace Citron
