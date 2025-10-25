#pragma once
#include <vector>

namespace Citron {

class NFuncDecl;
class MStmt;

namespace SyntaxIR0Translator {

class TranslationContext;

class BodyPhaseContext
{
public:
    void AddBody(NFuncDecl* funcDecl, std::vector<MStmt*>&& mStmts);
    void MarkFailed();
    TranslationContext MakeTranslationContext();
};

} // namespace SyntaxIR0Translator

} // namespace Citron
