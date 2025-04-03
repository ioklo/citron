export module Citron.SyntaxIR0Translator:BinOpQueryService;

import <memory>;
import <vector>;
import <unordered_map>;

import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export struct BinOpInfo
{
    RTypePtr operandType0;
    RTypePtr operandType1;
    RTypePtr resultType;
    RInternalBinaryOperator rOperator;

    BinOpInfo(const RTypePtr& operandType0, const RTypePtr& operandType1, const RTypePtr& resultType, RInternalBinaryOperator rOperator);
};

export class BinOpQueryService
{
    std::unordered_map<SBinaryOpKind, std::vector<BinOpInfo>> infos;

public:
    BinOpQueryService(RTypeFactory& factory);

    const std::vector<BinOpInfo>& GetInfos(SBinaryOpKind kind);
};

} // namespace Citron::SyntaxIR0Translator
