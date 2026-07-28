#pragma once
#include <span>
#include <memory>
#include <expected>
#include "RSymbol/RFuncDecl.h"

namespace Citron {

class SStmt;
struct MFuncBody;

using LoggerPtr = std::shared_ptr<class Logger>;
using DiagPtr = std::shared_ptr<struct Diag>;
using MFactoryPtr = std::shared_ptr<class MFactory>;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

using SmFactoryPtr = std::shared_ptr<class SmFactory>;
using BinOpQueryServicePtr = std::shared_ptr<class BinOpQueryService>;

class TranslateBodyContext
{
    LoggerPtr logger;
    MFactoryPtr mFactory;
    RFactoryPtr rFactory;
    SmFactoryPtr smFactory;
    BinOpQueryServicePtr binOpQueryService;

public:
    TranslateBodyContext(
        TakeRef<LoggerPtr> logger, TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory,
        TakeRef<SmFactoryPtr> smFactory, TakeRef<BinOpQueryServicePtr> binOpQueryService);
    ~TranslateBodyContext();

    std::expected<MFuncBody, DiagPtr> Translate(TakeRef<SmDeclContextPtr> declContext, RFuncDecl* funcDecl, bool bSeqFunc, std::span<SStmt*> mStmts);
    void MarkFailed();
};

} // namespace Citron
