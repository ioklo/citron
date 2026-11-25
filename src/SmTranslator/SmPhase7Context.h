#pragma once
#include <span>
#include <memory>
#include <expected>
#include "Infra/RefPtr.h"
#include "RSymbol/RFuncDecl.h"

namespace Citron {

class SStmt;
struct MFuncBody;

using LoggerPtr = std::shared_ptr<class Logger>;
using DiagPtr = std::shared_ptr<struct Diag>;
using MFactoryPtr = std::shared_ptr<class MFactory>;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmFactoryPtr = std::shared_ptr<class SmFactory>;
using BinOpQueryServicePtr = std::shared_ptr<class BinOpQueryService>;
class SmDeclContext;

class SmPhase7Context
{
    LoggerPtr logger;
    MFactoryPtr mFactory;
    RFactoryPtr rFactory;
    SmFactoryPtr smFactory;
    BinOpQueryServicePtr binOpQueryService;

public:
    SmPhase7Context(
        InRefPtr<Logger> logger, InRefPtr<RFactory> rFactory, InRefPtr<MFactory> mFactory,
        InRefPtr<SmFactory> smFactory, InRefPtr<BinOpQueryService> binOpQueryService);
    ~SmPhase7Context();

    std::expected<MFuncBody, DiagPtr> Translate(InRefPtr<SmDeclContext> declContext, RFuncDecl* funcDecl, bool bSeqFunc, std::span<SStmt*> mStmts);
    void MarkFailed();
};

} // namespace Citron
