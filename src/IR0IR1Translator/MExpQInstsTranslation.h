#pragma once
#include <expected>
#include <memory>

#include "QIR/QValues.h"

namespace Citron {

class MExp;
class QBlock;
class QFactory;
struct Diag;
using DiagPtr = std::shared_ptr<Diag>;

namespace IR0IR1Translator {
class QBodyContext;

// mExp를 여러개의 qInst로 번역해서 block에 집어넣는다.
std::expected<QValue, DiagPtr> TranslateMExpToQInsts(MExp* mExp, QBodyContext& bodyContext);
} // namespace IR0IR1Translator
} // namespace Citron
