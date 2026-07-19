#pragma once
#include "RSymbolConfig.h"
#include <vector>
#include <unordered_map>
#include "Infra/Ref.h"
#include "RNames.h"
#include "RDecl.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"
#include "RGenericsComponent.h"
#include "RTypeDeclContainerComponent.h"
#include "RFuncDeclContainerComponent.h"
#include "RDeclRes.h"
#include "RClassFuncDecl.h"

namespace Citron {

class RTypeArguments;
class RType_Class;
class RType_Interface;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class RClassDecl final : public RDecl, public RTypeDecl
{
    struct BaseTypes
    {
        RType_Class* baseClass;
        std::vector<RType_Interface> interfaces;
    };

    RTypeDeclOuter outer;
    RName name;    

    int trivialCtorIndex; // can be -1
    std::vector<RClassCtorDecl*> ctors;
    std::vector<RClassVarDecl*> vars;
    std::optional<BaseTypes> o_baseTypes;
    std::unordered_map<RName, RClassVarDecl*> varsMap;

    RGenericsComponent genericsComp;
    RTypeDeclContainerComponent typeDeclContainerComp;
    RFuncDeclContainerComponent<RClassFuncDecl, RMember_ClassFuncs> funcDeclContainerComp;
    RFactoryPtr rFactory;

public:
    RSYMBOL_API RClassDecl(RTypeDeclOuter outer, TakeRef<RName> name, TakeRef<RFactoryPtr> rFactory);
    void InitTypeParams(std::vector<RTypeParam*>&& typeParams) { return genericsComp.InitTypeParams(std::move(typeParams)); }

    RSYMBOL_API RClassVarDecl* GetUnboundVar(InRef<RName> name);
    RSYMBOL_API std::optional<RDeclRes_ClassVar> ResolveVar(RTypeArguments* typeArgs, InRef<RName> name);

public: // for RTypeDeclOuter
    RSYMBOL_API void AddType(RTypeDecl* typeDecl) { typeDeclContainerComp.AddType(typeDecl); }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RIdentifier GetIdentifier() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RTypeRes> ResolveInheritedTypeMember(RTypeArguments* typeArgs, InRef<RName> name) final;
    RSYMBOL_API std::optional<RDeclRes> ResolveInheritedMember(RTypeArguments* typeArgs, InRef<RName> name) final;

public: // from RTypeDecl
    RSYMBOL_API RDecl* RTypeDecl_GetDecl() final;
    RSYMBOL_API RType* GetOpenType() final;
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
};

} // namespace Citron