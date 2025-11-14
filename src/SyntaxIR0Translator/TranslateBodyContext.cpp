#include "TranslateBodyContext.h"

#include "Infra/Exceptions.h"
#include "TranslationContext.h"

namespace Citron::SyntaxIR0Translator {

void TranslateBodyContext::MarkFailed()
{
    throw NotImplementedException{};
}

TranslationContext TranslateBodyContext::MakeTranslationContext()
{
    throw NotImplementedException{};
}


}