#pragma once

namespace Citron {

namespace SyntaxIR0Translator {

class TranslationContext;

class BodyPhaseContext
{
public:
    void MarkFailed();
    TranslationContext MakeTranslationContext();
};

} // namespace SyntaxIR0Translator

} // namespace Citron