#pragma once
#include "IR0Config.h"

#include <vector>
#include <optional>
#include <memory>

#include "RDecl.h"
#include "RTypeDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "REnumElemDecl.h"
#include "RTypeDeclOuter.h"

namespace Citron
{

class REnumDecl
    : public RTypeDecl
{
    RTypeDeclOuterPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;
    std::vector<std::shared_ptr<REnumElemDecl>> elems;

    // std::unordered_map<std::string, int> elemsByName;

public:
    IR0_API REnumDecl(RTypeDeclOuterPtr outer, RAccessor accessor, RName name, std::vector<std::string> typeParams, size_t elemCount);
    IR0_API void AddElem(std::shared_ptr<REnumElemDecl> elem);
    
public:
    IR0_API RDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API RMemberPtr GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}

