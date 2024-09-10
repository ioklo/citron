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
    : public RDecl
    , public RTypeDecl
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
    
    RIdentifier GetIdentifier() override { return RIdentifier { name, (int)typeParams.size(), {} }; }

    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}

