#pragma once

#include <memory>
#include <vector>
#include <unordered_map>
#include "Syntax/Syntax.h"

namespace Citron {

class RFactory;
class RType;

enum class MExp_CallIntrinsicKind;
enum class MInitExp_CallIntrinsicKind;

struct BinOp_Exp { MExp_CallIntrinsicKind kind; };
struct BinOp_InitExp { MInitExp_CallIntrinsicKind kind; };

using BinOp = std::variant<BinOp_Exp, BinOp_InitExp>;

struct BinOpInfo
{
    RType* operandType0;
    RType* operandType1;
    RType* resultType;
    BinOp _operator;
};

class BinOpQueryService
{
    std::unordered_map<SBinaryOpKind, std::vector<BinOpInfo>> infos;

public:
    BinOpQueryService(RFactory& factory);
    const std::vector<BinOpInfo>& GetInfos(SBinaryOpKind kind);
};

using BinOpQueryServicePtr = std::shared_ptr<BinOpQueryService>;

} // namespace Citron