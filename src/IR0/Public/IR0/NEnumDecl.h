#pragma once
#include "IR0Config.h"

#include <vector>
#include <optional>
#include <memory>

#include "NDecl.h"
#include "NTypeDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "NEnumElemDecl.h"
#include "NTypeDeclOuter.h"

namespace Citron
{

class NEnumDecl
    : public NTypeDecl
{
    NTypeDeclOuterWPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;
    std::vector<std::shared_ptr<NEnumElemDecl>> elems;

    // std::unordered_map<std::string, int> elemsByName;

public:
    IR0_API NEnumDecl(NTypeDeclOuterWPtr outer, RAccessor accessor, RName name, std::vector<std::string> typeParams, size_t elemCount);
    IR0_API void AddElem(std::shared_ptr<NEnumElemDecl> elem);
    
public:
    RAccessor GetAccessor() override { return accessor; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API RMemberPtr GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}

