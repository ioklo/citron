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
#include "RDeclRes.h"

namespace Citron {

class RType_Trait;
class RTypeArguments;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class RStructCtorDecl;
class RStructDtorDecl;
class RStructFuncDecl;
class RStructVarDecl;

class RStructDecl final : public RDecl, public RTypeDecl
{
    RTypeDeclOuter outer; // outer with accessor
    RName name;

    std::vector<RStructCtorDecl*> ctors;
    RStructDtorDecl* dtor;
    int trivialCtorIndex; // can be -1

    std::vector<RStructVarDecl*> vars;
    std::optional<std::vector<RType_Trait*>> o_traits;
    std::unordered_map<RName, RStructVarDecl*> varsMap;

    RGenericsComponent genericsComp;
    RTypeDeclContainerComponent typeDeclContainerComp;
    RFuncDeclContainerComponent<RStructFuncDecl, RDeclRes_StructFuncs> funcDeclContainerComp;
    RFactoryPtr rFactory;

public:
    RSYMBOL_API RStructDecl(RTypeDeclOuter outer, TakeRef<RName> name, TakeRef<RFactoryPtr> rFactory);
    void InitTypeParams(std::vector<RTypeParamDecl*>&& typeParams) { return genericsComp.InitTypeParams(std::move(typeParams)); }
    RSYMBOL_API void InitTraits(std::vector<RType_Trait*>&& traits);

    void AddType(RTypeDecl* typeDecl) { typeDeclContainerComp.AddType(typeDecl); }
    RSYMBOL_API void AddCtor(RStructCtorDecl* decl);
    RSYMBOL_API void AddDtor(RStructDtorDecl* decl);
    RSYMBOL_API void AddFunc(RStructFuncDecl* decl) { funcDeclContainerComp.AddFunc(decl); }
    RSYMBOL_API void AddVar(RStructVarDecl* decl);

    std::span<RStructCtorDecl*> GetUnboundCtors() { return ctors; }
    std::span<RStructVarDecl*> GetUnboundVars() { return vars; }
    size_t GetVarCount() { return vars.size(); }
    RSYMBOL_API RStructVarDecl* GetUnboundVar(InRef<RName> name);
    RStructCtorDecl* GetUnboundTrivialCtor() { return trivialCtorIndex == -1 ? nullptr : ctors[trivialCtorIndex]; }
    RSYMBOL_API RStructCtorDecl* GetUnboundCopyCtor();

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) override;
    RSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;

public: // from RTypeDecl
    RSYMBOL_API RDecl* RTypeDecl_GetDecl() override;
    RSYMBOL_API RType* GetOpenType() override;
    RSYMBOL_API RDeclRes ToRDeclRes(RTypeArguments* typeArgs) override;
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) override;
};

} // namespace Citron

