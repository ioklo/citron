#pragma once
#include <span>
#include <memory>
#include <expected>
#include <vector>

namespace Citron {

class NFuncDecl;
class SStmt;
struct MFuncBody;

using LoggerPtr = std::shared_ptr<class Logger>;
using DiagPtr = std::shared_ptr<struct Diag>;
using MFactoryPtr = std::shared_ptr<class MFactory>;
using RFactoryPtr = std::shared_ptr<class RFactory>;

namespace SyntaxIR0Translator {

class TranslationContext;
using SRTFactoryPtr = std::shared_ptr<class SRTFactory>;
using BinOpQueryServicePtr = std::shared_ptr<class BinOpQueryService>;

class TranslateBodyContext
{
    LoggerPtr logger;
    MFactoryPtr mFactory;
    RFactoryPtr rFactory;
    SRTFactoryPtr srtFactory;
    BinOpQueryServicePtr binOpQueryService;

public:
    TranslateBodyContext(
        const LoggerPtr& logger, const RFactoryPtr& rFactory, const MFactoryPtr& mFactory,
        const SRTFactoryPtr& srtFactory, const BinOpQueryServicePtr& binOpQueryService);
    ~TranslateBodyContext();

    std::expected<MFuncBody, DiagPtr> Translate(NFuncDecl* funcDecl, std::span<SStmt*> mStmts);
    void MarkFailed();
    TranslationContext MakeTranslationContext();
};

} // namespace SyntaxIR0Translator

} // namespace Citron
