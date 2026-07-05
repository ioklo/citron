#pragma once
#include "RSymbolConfig.h"
#include <vector>
#include <optional>

#include "Infra/Ref.h"
#include "RNames.h"
#include "RDeclRes.h"

namespace Citron {

class RTypeParamDecl;
class RTypeDecl;

class RGenericsComponent
{
    std::optional<std::vector<RTypeParamDecl*>> o_typeParams;

public:
    RSYMBOL_API RGenericsComponent();
    RSYMBOL_API void InitTypeParams(std::vector<RTypeParamDecl*>&& typeParams);
    size_t GetTypeParamCount() { return o_typeParams->size(); }
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index);
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount);
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount);
};


} // namespace Citron