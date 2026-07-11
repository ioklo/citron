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
        TakeRef<LoggerPtr> logger, TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory,
        TakeRef<SRTFactoryPtr> srtFactory, TakeRef<BinOpQueryServicePtr> binOpQueryService);
    ~TranslateBodyContext();

    std::expected<MFuncBody, DiagPtr> Translate(RFuncDecl* funcDecl, std::span<SStmt*> mStmts);
    void MarkFailed();
};

} // namespace Citron
