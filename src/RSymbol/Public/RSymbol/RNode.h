#pragma once
#include "RSymbolConfig.h"
#include <vector>

#include "RNodeDecl.h"
#include "RIdentifier.h"

namespace Citron {

class RTypeArguments;
class RFactory;
enum class RAccessor;

// SymbolTree
class RNode
{   
    RNode* outer;
    std::vector<RNode*> members;
    RAccessor accessor;
    RName name;
    RNodeDecl decl;

public:
    RSYMBOL_API bool IsDescendantOf(RNode* node);
    RSYMBOL_API bool CanAccess(RNode* target); // target이 이 노드에 접근 가능한가
    RSYMBOL_API size_t GetAllTypeParamCount();

    RSYMBOL_API RTypeArguments* MakeOpenTypeArgs(RFactory& factory);

    RNode* GetOuter() { return outer; }
    RAccessor GetAccessor() { return accessor; }
    RName GetName() { return name; }
};


} // namespace Citron
