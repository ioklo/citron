#pragma once

namespace Citron {

struct MLoc_ClassVar;
struct MLoc_StructVar;
struct ImExp_ClassVar;
struct ImExp_StructVar;
struct TranslationContexts;

MLoc_ClassVar* TranslateImExp_ClassVarToMLoc_ClassVar(ImExp_ClassVar* imExp, TranslationContexts& contexts);
MLoc_StructVar* TranslateImExp_StructVarToMLoc_StructVar(ImExp_StructVar* imExp, TranslationContexts& contexts);


} // namespace Citron
