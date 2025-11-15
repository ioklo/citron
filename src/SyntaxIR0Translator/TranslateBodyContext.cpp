#include "TranslateBodyContext.h"

#include "Infra/Exceptions.h"
#include "TranslationContext.h"
#include "SStmtToMStmtTranslation.h"

namespace Citron::SyntaxIR0Translator {

void TranslateBodyContext::Translate(NFuncDecl* funcDecl, std::span<SStmt*> mStmts)
{   
    throw NotImplementedException{};
    /*auto tContext = TranslationContext::New(funcDecl->GetNDecl()->GetRDecl(), )
    TranslateSBodyToMStmts(mStmts, tContext);*/
}

void TranslateBodyContext::MarkFailed()
{
    throw NotImplementedException{};
}

TranslationContext TranslateBodyContext::MakeTranslationContext()
{
    throw NotImplementedException{};
}


}