#pragma once
#include "RSymbolConfig.h"

#include "Infra/Ref.h"
#include "RTypeDeclOuter.h"
#include "RGenericsComponent.h"
#include "RTypeDeclContainerComponent.h"
#include "RFuncDeclContainerComponent.h"
#include "RDecl.h"
#include "RTypeDecl.h"
#include "RStructFuncDecl.h"
#include "RDeclKey.h"

namespace Citron {

class RType_Trait;
class RTypeArguments;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class RStructCtorDecl;
class RStructDtorDecl;
class RStructFuncDecl;
class RStructVarDecl;

struct NStructInfo;

class RStructDecl final : public RDecl, public RTypeDecl
{
    RDeclKey key;
    RTypeDeclOuter outer; // owner with accessor
    RName name;

    std::vector<RStructCtorDecl*> ctors;
    RStructDtorDecl* dtor;
    int trivialCtorIndex; // can be -1

    std::vector<RStructVarDecl*> vars;
    std::optional<std::vector<RType_Trait*>> o_traits;
    std::unordered_map<RName, RStructVarDecl*> varsMap;
    NStructInfo* structInfo;

    RGenericsComponent genericsComp;
    RTypeDeclContainerComponent typeDeclContainerComp;
    RFuncDeclContainerComponent<RStructFuncDecl, RMember_StructFuncs> funcDeclContainerComp;
    RFactoryPtr rFactory;

public:
    RSYMBOL_API RStructDecl(RDeclKey&& key, RTypeDeclOuter outer, RName&& name, TakeRef<RFactoryPtr> rFactory);
    void InitTypeParams(std::vector<RTypeParam*>&& typeParams) { return genericsComp.InitTypeParams(std::move(typeParams)); }
    RSYMBOL_API void InitTraits(std::vector<RType_Trait*>&& traits);

    void AddType(RTypeDecl* typeDecl) { typeDeclContainerComp.AddType(typeDecl); }
    RSYMBOL_API void AddCtor(RStructCtorDecl* decl);
    RSYMBOL_API void AddDtor(RStructDtorDecl* decl);
    RSYMBOL_API void AddFunc(RStructFuncDecl* decl) { funcDeclContainerComp.AddFunc(decl); }
    RSYMBOL_API void AddVar(RStructVarDecl* decl);

    NStructInfo* GetNStructInfo() { return structInfo; }

    std::span<RStructCtorDecl*> GetUnboundCtors() { return ctors; }
    std::span<RStructVarDecl*> GetUnboundVars() { return vars; }
    size_t GetVarCount() { return vars.size(); }
    RSYMBOL_API RStructVarDecl* GetUnboundVar(InRef<RName> name);
    RStructCtorDecl* GetUnboundTrivialCtor() { return trivialCtorIndex == -1 ? nullptr : ctors[trivialCtorIndex]; }
    RSYMBOL_API RStructCtorDecl* GetUnboundCopyCtor();

    RType* GetOpenType();

public: // from RDecl
    RSYMBOL_API RDeclKey& GetDeclKey() final;
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RName* TryGetName() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;

public: // from RTypeDecl
    RSYMBOL_API RDecl* RTypeDecl_GetDecl() final;
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
};

} // namespace Citron

