#pragma once

#include <vector>
#include <memory>

namespace Citron {

struct ImExp;
struct IrExp;

// ImExp, IrExp
class SRTFactory
{
    std::vector<std::unique_ptr<ImExp>> imExps;
    std::vector<std::unique_ptr<IrExp>> irExps;

public:
    SRTFactory();
    ~SRTFactory();

    template<typename TImExp, typename... TArgs> requires std::derived_from<TImExp, ImExp>
    TImExp* MakeImExp(TArgs&&... args)
    {
        auto imExp = std::make_unique<TImExp>(std::forward<TArgs>(args)...);
        auto* pImExp = imExp.get();
        imExps.push_back(std::move(imExp));
        return pImExp;
    }

    template<typename TIrExp, typename... TArgs> requires std::derived_from<TIrExp, IrExp>
    TIrExp* MakeIrExp(TArgs&&... args)
    {
        auto irExp = std::make_unique<TIrExp>(std::forward<TArgs>(args)...);
        auto* pIrExp = irExp.get();
        irExps.push_back(std::move(irExp));
        return pIrExp;
    }
};

using SRTFactoryPtr = std::shared_ptr<SRTFactory>;

} // namespace Citron