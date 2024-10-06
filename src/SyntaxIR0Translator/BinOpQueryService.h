#pragma once

#include <memory>
#include <vector>
#include <unordered_map>
#include <IR0/RExp.h>
#include <Syntax/Syntax.h>

namespace Citron {

class RType;
using RTypePtr = std::shared_ptr<RType>;

namespace SyntaxIR0Translator {

struct BinOpInfo
{
    RTypePtr operandType0;
    RTypePtr operandType1;
    RTypePtr resultType;
    RInternalBinaryOperator rOperator;

    BinOpInfo(const RTypePtr& operandType0, const RTypePtr& operandType1, const RTypePtr& resultType, RInternalBinaryOperator rOperator);
};

class BinOpQueryService
{
    std::unordered_map<SBinaryOpKind, std::vector<BinOpInfo>> infos;

public:
    BinOpQueryService(RTypeFactory& factory);

    const std::vector<BinOpInfo>& GetInfos(SBinaryOpKind kind);
};

} // namespace SyntaxIR0Translator
} // namespace Citron
