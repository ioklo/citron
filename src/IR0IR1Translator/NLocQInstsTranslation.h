#pragma once
#include <expected>
#include <memory>

namespace Citron {

class Diag;
using DiagPtr = std::shared_ptr<Diag>;

class NLoc;

class QValue;
class QBlock;
class QFactory;


namespace IR0IR1Translator {
class QBodyContext;

std::expected<QValue*, DiagPtr> TranslateNLocToQInsts(NLoc* loc, QBlock* block, QBodyContext* bodyContext, QFactory* factory);

} // namespace IR0IR1Translator
} // namespace Citron