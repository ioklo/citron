#pragma once
#include "IR0Config.h"

#include <vector>
#include <optional>
#include <memory>
#include <unordered_map>

#include "NDecl.h"
#include "NTypeDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "NEnumElemDecl.h"
#include "NTypeDeclOuter.h"

#include "REnumDecl.h"
#include "RMember.h"

namespace Citron
{

class NEnumDecl
    : public NDecl
    , public NTypeDecl
    , public REnumDecl
{
    NTypeDeclOuterWPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;
    std::vector<std::shared_ptr<NEnumElemDecl>> elems;
    std::unordered_map<std::string, std::shared_ptr<NEnumElemDecl>> elemsMap;

    // std::unordered_map<std::string, int> elemsByName;

public:
    IR0_API NEnumDecl(NTypeDeclOuterWPtr outer, RAccessor accessor, RName name, std::vector<std::string> typeParams, size_t elemCount);
    IR0_API void AddElem(std::shared_ptr<NEnumElemDecl>&& elem);
    
public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }    
    RMember ToRMember(const std::shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }
};

}

