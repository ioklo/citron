#pragma once
#include <memory>
#include <optional>
#include <vector>
#include "QIR/QArgs.h"

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;
using QFactoryPtr = std::shared_ptr<class QFactory>;
using MqFactoryPtr = std::shared_ptr<class MqFactory>;
using QAbiPtr = std::shared_ptr<class QAbi>;
class QBodyContext;
enum class QInst_IntrinsicKind;

struct QTranslationContexts
{
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;
    MqFactoryPtr mqFactory;
    QAbiPtr qAbi;
    QBodyContext& bodyContext;
};

} // namespace Citron
