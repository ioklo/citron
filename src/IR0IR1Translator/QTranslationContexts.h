#pragma once
#include <memory>

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;
class QBodyContext;

struct QTranslationContexts
{
    RFactoryPtr rFactory;
    QBodyContext& bodyContext;
};


} // namespace Citron
