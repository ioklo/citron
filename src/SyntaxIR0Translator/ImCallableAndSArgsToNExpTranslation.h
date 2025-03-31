#pragma once

import <memory>;
import <vector>;

namespace Citron {

using SExpPtr = std::shared_ptr<class SExp>;
using SArgumentsPtr = std::shared_ptr<class SArguments>;
using LoggerPtr = std::shared_ptr<class Logger>;
using NExpPtr = std::shared_ptr<class NExp>;

class RTypeFactory;

namespace SyntaxIR0Translator {

class ImExp;
class TranslationContext;

NExpPtr TranslateImCallableAndSArgsToNExp(ImExp& imCallable, const SExpPtr& sCallable, const SArgumentsPtr& sArgs, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron