#pragma once
#include "RSymbolConfig.h"
#include <vector>
#include <memory>
#include <unordered_map>
#include "RDecl.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"
#include "RGenericsComponent.h"
#include "RDeclKey.h"

namespace Citron {

class REnumElemDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class REnumDecl final : public RDecl, public RTypeDecl
{
    RDeclKey key;
    RTypeDeclOuter outer;
    RName name;
    std::vector<REnumElemDecl*> elems;
    std::unordered_map<RName, REnumElemDecl*> elemsMap;
    RFactoryPtr rFactory;

    RGenericsComponent genericsComp;

public:
    RSYMBOL_API REnumDecl(RDeclKey&& key, RTypeDeclOuter outer, RName&& name, TakeRef<RFactoryPtr> rFactory);
    void InitTypeParams(std::vector<RTypeParam*>&& typeParams) { genericsComp.InitTypeParams(std::move(typeParams)); }
    RSYMBOL_API void AddElem(REnumElemDecl* elem);

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
