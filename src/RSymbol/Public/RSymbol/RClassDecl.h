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
    RFuncDeclContainerComponent<RClassFuncDecl, RDeclRes_ClassFuncs> funcDeclContainerComp;
    RFactoryPtr rFactory;

public:
    RSYMBOL_API RClassDecl(RTypeDeclOuter outer, TakeRef<RName> name, TakeRef<RFactoryPtr> rFactory);
    void InitTypeParams(std::vector<RTypeParamDecl*>&& typeParams) { return genericsComp.InitTypeParams(std::move(typeParams)); }

    RSYMBOL_API std::optional<RDeclRes_ClassVar> ResolveVar(RTypeArguments* typeArgs, InRef<RName> name);

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