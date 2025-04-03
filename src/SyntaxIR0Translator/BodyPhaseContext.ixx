export module Citron.SyntaxIR0Translator:BodyPhaseContext;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export class BodyPhaseContext
{
public:
    void MarkFailed();
    TranslationContext MakeTranslationContext();
};

} // namespace Citron::SyntaxIR0Translator