#pragma once
#include <memory>
#include <optional>
#include <vector>
#include "QIR/QArgs.h"

namespace Citron {

using RFactoryPtr = std::shared_ptr<class RFactory>;
using QFactoryPtr = std::shared_ptr<class QFactory>;
using MqFactoryPtr = std::shared_ptr<class MqFactory>;
using QAbiPtr = std::shared_ptr<class MqAbi>;
class MqBodyContext;
enum class QInst_IntrinsicKind;

struct MqTranslationContexts
{
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;
    MqFactoryPtr mqFactory;
    QAbiPtr qAbi;
    MqBodyContext& bodyContext;
};

} // namespace Citron
