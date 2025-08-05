#pragma once
#include <expected>
#include <memory>

namespace Citron {

class NExp;
class QValue;
class QBlock;
class QFactory;
class Diag;
using DiagPtr = std::shared_ptr<Diag>;

namespace IR0IR1Translator {
class QBodyContext;

// nExp를 여러개의 qInst로 번역해서 block에 집어넣는다.
std::expected<QValue*, DiagPtr> TranslateNExpToQInsts(NExp* nExp, QBlock* block, QBodyContext* bodyContext, QFactory* factory);
} // namespace IR0IR1Translator
} // namespace Citron
