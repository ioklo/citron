#pragma once
#include <memory>

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;
using QFactoryPtr = std::shared_ptr<class QFactory>;
using QAbiPtr = std::shared_ptr<class QAbi>;
class QBodyContext;

struct QTranslationContexts
{
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;
    QAbiPtr qAbi;
    QBodyContext& bodyContext;
};


} // namespace Citron
