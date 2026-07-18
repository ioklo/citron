#pragma once
#include "RSymbolConfig.h"
#include <memory>
#include "RDecl.h"
#include "RTypeDecl.h"
#include "RNames.h"

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;

class RTypeParam
{
    // RTypeParamOwner를 만들지, 그냥 RDecl로 할지. 일단 쓰이는데가 있을때까지는 RDecl로 한다
    RDecl* owner;
    RName name;
    size_t globalIndex;
    RFactoryPtr rFactory;

public:
    RSYMBOL_API RTypeParam(RDecl* outer, RName&& name, size_t globalIndex, TakeRef<RFactoryPtr> rFactory);
    RName& GetName() { return name; }
    size_t GetGlobalIndex() { return globalIndex; }
};


} // namespace Citron
