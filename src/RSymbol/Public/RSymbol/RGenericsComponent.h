#pragma once
#include "RSymbolConfig.h"
#include <vector>
#include <optional>

#include "Infra/Ref.h"

namespace Citron {

class RTypeParam;
class RTypeDecl;
class RName;

class RGenericsComponent
{
    std::optional<std::vector<RTypeParam*>> o_typeParams;

public:
    RSYMBOL_API RGenericsComponent();
    RSYMBOL_API void InitTypeParams(std::vector<RTypeParam*>&& typeParams);
    size_t GetTypeParamCount() { return o_typeParams->size(); }
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index);
    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name);
};


} // namespace Citron