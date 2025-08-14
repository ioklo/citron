#pragma once

#include <vector>
#include <memory>

namespace Citron {

namespace SyntaxIR0Translator {

class ImExp;
class IrExp;
class ReExp;

// ImExp, IrExp, ReExp
class SRTFactory
{
    std::vector<std::unique_ptr<ImExp>> imExps;
    std::vector<std::unique_ptr<IrExp>> irExps;
    std::vector<std::unique_ptr<ReExp>> reExps;

public:
    SRTFactory();
    ~SRTFactory();

    template<typename TImExp, typename... TArgs> requires std::derived_from<TImExp, ImExp>
    constexpr TImExp* MakeImExp(TArgs&&... args)
    {
        auto imExp = std::make_unique<TImExp>(std::forward<TArgs>(args)...);
        auto* pImExp = imExp.get();
        imExps.push_back(std::move(imExp));
        return pImExp;
    }

    template<typename TIrExp, typename... TArgs> requires std::derived_from<TIrExp, IrExp>
    constexpr TIrExp* MakeIrExp(TArgs&&... args)
    {
        auto irExp = std::make_unique<TIrExp>(std::forward<TArgs>(args)...);
        auto* pIrExp = irExp.get();
        irExps.push_back(std::move(irExp));
        return pIrExp;
    }

    template<typename TReExp, typename... TArgs> requires std::derived_from<TReExp, ReExp>
    constexpr TReExp* MakeReExp(TArgs&&... args)
    {
        auto reExp = std::make_unique<TReExp>(std::forward<TArgs>(args)...);
        auto* pReExp = reExp.get();
        reExps.push_back(std::move(reExp));
        return pReExp;
    }
};

using SRTFactoryPtr = std::shared_ptr<SRTFactory>;

} // namespace SyntaxIR0Translator

} // namespace Citron