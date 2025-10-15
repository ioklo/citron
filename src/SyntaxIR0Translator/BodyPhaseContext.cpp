#include "BodyPhaseContext.h"

#include "Infra/Exceptions.h"
#include "TranslationContext.h"

namespace Citron::SyntaxIR0Translator {

void BodyPhaseContext::MarkFailed()
{
    throw NotImplementedException{};
}

TranslationContext BodyPhaseContext::MakeTranslationContext()
{
    throw NotImplementedException{};
}


}