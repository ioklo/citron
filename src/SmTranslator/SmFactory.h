#pragma once

#include <vector>
#include <memory>

namespace Citron {

struct ImExp;
struct IrExp;
class SmType;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

struct SmFactoryPrivateData;
// ImExp, IrExp
class SmFactory
{
    std::vector<std::unique_ptr<ImExp>> imExps;
    std::vector<std::unique_ptr<IrExp>> irExps;

    std::unique_ptr<SmFactoryPrivateData> privateData;

public:
    SmFactory();
    ~SmFactory();

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

    template<typename TSmType, typename... TArgs> requires (!std::same_as<std::remove_cvref_t<TSmType>, SmType>) &&  std::constructible_from<SmType, TSmType&&>
    SmType* MakeSmType(TArgs&&... args)
    {
        return MakeSmType(TSmType{std::forward<TArgs>(args)...});
    }

private:
    SmType* MakeSmType(SmType&& type);
};

using SmFactoryPtr = std::shared_ptr<SmFactory>;

} // namespace Citron