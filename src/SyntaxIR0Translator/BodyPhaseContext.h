#pragma once

namespace Citron::SyntaxIR0Translator {

class TranslationContext;

class BodyPhaseContext
{
public:
    void MarkFailed();
    TranslationContext MakeTranslationContext();
};

} // namespace Citron::SyntaxIR0Translator