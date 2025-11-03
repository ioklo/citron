#pragma once

#include <memory>
#include <vector>
#include <unordered_map>
#include "Syntax/Syntax.h"

namespace Citron {

class RFactory;
class RType;
enum class MInternalBinaryOperator;

namespace SyntaxIR0Translator {

struct BinOpInfo
{
    RType* operandType0;
    RType* operandType1;
    RType* resultType;
    MInternalBinaryOperator rOperator;

    BinOpInfo(RType* operandType0, RType* operandType1, RType* resultType, MInternalBinaryOperator rOperator);
};

class BinOpQueryService
{
    std::unordered_map<SBinaryOpKind, std::vector<BinOpInfo>> infos;

public:
    BinOpQueryService(RFactory& factory);

    const std::vector<BinOpInfo>& GetInfos(SBinaryOpKind kind);
};

} // namespace SyntaxIR0Translator

} // namespace Citron