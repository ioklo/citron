#pragma once

#include <memory>
#include <vector>

namespace Citron {

class STypeExp;

class RType;
using RTypePtr = std::shared_ptr<RType>;

enum class SBinaryOpKind;

namespace SyntaxIR0Translator {

struct BinOpInfo;
class BinOpQueryService;


class ScopeContext
{
    std::shared_ptr<BinOpQueryService> binOpQueryService;

public:
    ScopeContext(const std::shared_ptr<BinOpQueryService>& binOpQueryService);

    bool IsFailed();
    RTypePtr MakeType(STypeExp& typeExp);
    const std::vector<BinOpInfo>& GetBinOpInfos(SBinaryOpKind kind);
};

using ScopeContextPtr = std::shared_ptr<ScopeContext>;

} // namespace SyntaxIR0Translator

} // namespace Citron