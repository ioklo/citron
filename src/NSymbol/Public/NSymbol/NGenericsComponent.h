#pragma once
#include "NSymbolConfig.h"
#include <vector>
#include <optional>
#include "RSymbol/RNames.h"
#include "RSymbol/RMember.h"

namespace Citron {

class RTypeParamDecl;
class RTypeDecl;
class NTypeParamDecl;
class NTypeDecl;

class NGenericsComponent
{   
    std::optional<std::vector<NTypeParamDecl*>> o_typeParams;

public:
    NSYMBOL_API NGenericsComponent();
    NSYMBOL_API void InitTypeParams(std::vector<NTypeParamDecl*>&& typeParams);
    size_t GetTypeParamCount() { return o_typeParams->size(); }
    NSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index);
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount);
    NSYMBOL_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount);
};


} // namespace Citron