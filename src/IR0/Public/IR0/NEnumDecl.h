#pragma once

#include "IR0Config.h"

#include <vector>
#include <optional>
#include <unordered_map>

#include "REnumDecl.h"

#include "NDecl.h"
#include "NTypeDecl.h"
#include "NEnumElemDecl.h"
#include "NTypeDeclOuter.h"

namespace Citron
{

class NEnumDecl
    : public NDecl
    , public NTypeDecl
    , public REnumDecl
{
    NTypeDeclOuter* outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;
    std::vector<NEnumElemDecl*> elems;
    std::unordered_map<std::string, NEnumElemDecl*> elemsMap;

    // std::unordered_map<std::string, int> elemsByName;

public:
    IR0_API NEnumDecl(NTypeDeclOuter* outer, RAccessor accessor, RName name, std::vector<std::string> typeParams, size_t elemCount);
    IR0_API void AddElem(NEnumElemDecl* elem);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RMember ToRMember(RTypeArguments* typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory) override;

    // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }
};

}

