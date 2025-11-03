#pragma once
#include <expected>
#include <memory>

namespace Citron {

struct Diag;
using DiagPtr = std::shared_ptr<Diag>;

class MLoc;

class QValue;
class QBlock;
class QFactory;


namespace IR0IR1Translator {
class QBodyContext;

std::expected<QValue, DiagPtr> TranslateMLocToQInsts(MLoc* loc, QBodyContext& bodyContext);

} // namespace IR0IR1Translator
} // namespace Citron