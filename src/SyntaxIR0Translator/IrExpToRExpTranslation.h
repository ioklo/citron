#pragma once

#include <memory>

namespace Citron {

class RExp;
using RExpPtr = std::shared_ptr<RExp>;

namespace SyntaxIR0Translator {

class IrExp;
using IrExpPtr = std::shared_ptr<IrExp>;

RExpPtr TranslateIrExpToRExp(IrExpPtr irExp);

} // SyntaxIR0Translator 

} // Citron