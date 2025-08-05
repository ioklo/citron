#pragma once
#include "IR0IR1TranslatorConfig.h"

#include <memory>

namespace Citron {

class NModule;
class QModule;
class QFactory;
class Diag;
using DiagPtr = std::shared_ptr<Diag>;

// body만 바꾸면 되는데
IR0IR1TRANSLATOR_API QModule* Translate(NModule *nModule, DiagPtr diag, QFactory* factory);

}