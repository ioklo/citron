#pragma once
#include <span>

namespace Citron {

class NFuncDecl;
class SStmt;

namespace SyntaxIR0Translator {

class TranslationContext;

class TranslateBodyContext
{
public:
    void Translate(NFuncDecl* funcDecl, std::span<SStmt*> mStmts);
    void MarkFailed();
    TranslationContext MakeTranslationContext();
};

} // namespace SyntaxIR0Translator

} // namespace Citron
