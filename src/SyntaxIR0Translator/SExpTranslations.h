#pragma once
#include <expected>
#include <memory>
#include "ReExp.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class SExp_String;
class SExp_IntLiteral;
class SExp_BoolLiteral;
class SExp_NullLiteral;
class SExp_BinaryOp;
class SExp_UnaryOp;
class SExp_Lambda;
class SExp_Indexer;
class SExp_List;
class SExp_New;
class SExp_Shared;
class SExp_Is;
class SExp_As;
class RType;
struct MExp_IntLiteral;
struct MExp_BoolLiteral;
struct MExp_Is;
struct MLoc_ListIndexer;
struct MInitExp_String;
struct MInitExp_NewClass;
struct MInitExp_Shared;
struct MInitExp_As;
struct TranslationContexts;

std::expected<MInitExp_String*, DiagPtr> TranslateSExp_StringToMInitExp_String(SExp_String* sExp, TranslationContexts& contexts);
std::expected<MExp_IntLiteral*, DiagPtr> TranslateSExp_IntLiteralToMExp_IntLiteral(SExp_IntLiteral* sExp, TranslationContexts& contexts);
std::expected<MExp_BoolLiteral*, DiagPtr> TranslateSExp_BoolLiteralToMExp_BoolLiteral(SExp_BoolLiteral* sExp, TranslationContexts& contexts);
// MExp, InitExp양쪽 다 리턴 가능한 경우, ReExp를 리턴하도록 한다
std::expected<ReExp, DiagPtr> TranslateSExp_NullLiteralToReExp(SExp_NullLiteral* sExp, RType* hintType, TranslationContexts& contexts);
std::expected<ReExp, DiagPtr> TranslateSExp_BinaryOpToReExp(SExp_BinaryOp* sExp, TranslationContexts& contexts);
std::expected<ReExp, DiagPtr> TranslateSExp_UnaryOpToReExp(SExp_UnaryOp* sExp, RType* hintType, TranslationContexts& contexts);
std::expected<ReExp, DiagPtr> TranslateSExp_LambdaToReExp(SExp_Lambda* sExp, TranslationContexts& contexts);
std::expected<MLoc_ListIndexer*, DiagPtr> TranslateSExp_IndexerToMLoc_ListIndexer(SExp_Indexer* sExp, TranslationContexts& contexts);
std::expected<MInitExp*, DiagPtr> TranslateSExp_ListToMInitExp(SExp_List* exp, RType* hintType, TranslationContexts& contexts);

std::expected<MInitExp_NewClass*, DiagPtr> TranslateSExp_NewToMInitExp_NewClass(SExp_New* exp, TranslationContexts& contexts);
std::expected<MInitExp_Shared*, DiagPtr> TranslateSExp_SharedToMInitExp_Shared(SExp_Shared* sExp, RType* hintType, TranslationContexts& contexts);

std::expected<MExp_Is*, DiagPtr> TranslateSExp_IsToMExp_Is(SExp_Is* sExp, TranslationContexts& contexts);
std::expected<MInitExp_As*, DiagPtr> TranslateSExp_AsToMInitExp_As(SExp_As* sExp, TranslationContexts& contexts);

} // namespace Citron