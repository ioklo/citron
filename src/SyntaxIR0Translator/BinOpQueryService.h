#pragma once

#include <memory>
#include <vector>
#include <unordered_map>
#include "Syntax/Syntax.h"

namespace Citron {

class IR0Factory;
class RType;
enum class NInternalBinaryOperator;

namespace SyntaxIR0Translator {

struct BinOpInfo
{
    RType* operandType0;
    RType* operandType1;
    RType* resultType;
    NInternalBinaryOperator rOperator;

    BinOpInfo(RType* operandType0, RType* operandType1, RType* resultType, NInternalBinaryOperator rOperator);
};

class BinOpQueryService
{
    std::unordered_map<SBinaryOpKind, std::vector<BinOpInfo>> infos;

public:
    BinOpQueryService(IR0Factory& factory);

    const std::vector<BinOpInfo>& GetInfos(SBinaryOpKind kind);
};

} // namespace SyntaxIR0Translator

} // namespace Citron