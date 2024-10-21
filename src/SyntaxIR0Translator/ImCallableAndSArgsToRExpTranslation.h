#pragma once

#include <memory>
#include <vector>

namespace Citron {

using SExpPtr = std::shared_ptr<class SExp>;
using SArgumentsPtr = std::shared_ptr<class SArguments>;
using LoggerPtr = std::shared_ptr<class Logger>;
using RExpPtr = std::shared_ptr<class RExp>;

class RTypeFactory;

namespace SyntaxIR0Translator {

class ImExp;
class TranslationContext;

RExpPtr TranslateImCallableAndSArgsToRExp(ImExp& imCallable, const SExpPtr& sCallable, const SArgumentsPtr& sArgs, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron