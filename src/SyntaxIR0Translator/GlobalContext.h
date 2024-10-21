#pragma once
#include <memory>
#include <vector>

#include <IR0/RFuncDecl.h>
#include <IR0/RFuncReturn.h>

namespace Citron {

class MModuleDecl;

using LoggerPtr = std::shared_ptr<class Logger>;
using RTypeFactoryPtr = std::shared_ptr<class RTypeFactory>;
enum class SBinaryOpKind;

namespace SyntaxIR0Translator {

class BinOpQueryService;
struct BinOpInfo;
class CloneContext;
class UpdateContext;

using GlobalContextPtr = std::shared_ptr<class GlobalContext>;
using ModuleDeclsPtr = std::shared_ptr<class ModuleDecls>;

class GlobalContext
{
    ModuleDeclsPtr moduleDecls;

    LoggerPtr logger;
    RTypeFactoryPtr factory;

    std::shared_ptr<BinOpQueryService> binOpQueryService;

public:
    GlobalContext(const ModuleDeclsPtr& mModuleDecls, const LoggerPtr& logger, const RTypeFactoryPtr& factory, const std::shared_ptr<BinOpQueryService>& binOpQueryService);

    GlobalContextPtr Clone(CloneContext& cloneContext);
    void Update(const GlobalContextPtr& src, UpdateContext& updateContext);    

public:
    const std::vector<BinOpInfo>& GetBinOpInfos(SBinaryOpKind kind);

    
};

} // namespace SyntaxIR0Translator

} // namespace Citron