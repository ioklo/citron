#pragma once
#include <memory>

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;
using QFactoryPtr = std::shared_ptr<class QFactory>;
class QBodyContext;

struct QTranslationContexts
{
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;
    QBodyContext& bodyContext;
};


} // namespace Citron
