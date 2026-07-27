#pragma once

namespace Citron {

struct MLoc_ClassVar;
struct MLoc_StructVar;
struct ImExp_ClassVar;
struct ImExp_StructVar;
struct SmTranslationContexts;

MLoc_ClassVar* TranslateImExp_ClassVarToMLoc_ClassVar(ImExp_ClassVar* imExp, SmTranslationContexts& contexts);
MLoc_StructVar* TranslateImExp_StructVarToMLoc_StructVar(ImExp_StructVar* imExp, SmTranslationContexts& contexts);


} // namespace Citron
