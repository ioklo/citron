#include "pch.h"
#include "BinOpQueryService.h"

#include <IR0/RTypeFactory.h>

using namespace std;

namespace Citron::SyntaxIR0Translator {

BinOpInfo::BinOpInfo(const RTypePtr& operandType0, const RTypePtr& operandType1, const RTypePtr& resultType, RInternalBinaryOperator rOperator)
    : operandType0(operandType0), operandType1(operandType1), resultType(resultType), rOperator(rOperator)
{
}

BinOpQueryService::BinOpQueryService(RTypeFactory& factory)
{   
    auto intType = factory.MakeIntType();
    auto boolType = factory.MakeBoolType();
    auto stringType = factory.MakeStringType();

    infos.emplace(SBinaryOpKind::Multiply, vector<BinOpInfo>{ {intType, intType, intType, RInternalBinaryOperator::Multiply_Int_Int_Int } });
    infos.emplace(SBinaryOpKind::Divide, vector<BinOpInfo>{ {intType, intType, intType, RInternalBinaryOperator::Divide_Int_Int_Int } });
    infos.emplace(SBinaryOpKind::Modulo, vector<BinOpInfo>{ {intType, intType, intType, RInternalBinaryOperator::Modulo_Int_Int_Int}});
    infos.emplace(SBinaryOpKind::Add, vector<BinOpInfo>{
        { intType, intType, intType, RInternalBinaryOperator::Add_Int_Int_Int},
        { stringType, stringType, stringType, RInternalBinaryOperator::Add_String_String_String },
    });

    infos.emplace(SBinaryOpKind::Subtract, vector<BinOpInfo>{ { intType, intType, intType, RInternalBinaryOperator::Subtract_Int_Int_Int } });

    infos.emplace(SBinaryOpKind::LessThan, vector<BinOpInfo>{
        { intType, intType, boolType, RInternalBinaryOperator::LessThan_Int_Int_Bool },
        { stringType, stringType, boolType, RInternalBinaryOperator::LessThan_String_String_Bool }
    });

    infos.emplace(SBinaryOpKind::GreaterThan, vector<BinOpInfo>{
        { intType, intType, boolType, RInternalBinaryOperator::GreaterThan_Int_Int_Bool },
        { stringType, stringType, boolType, RInternalBinaryOperator::GreaterThan_String_String_Bool }
    });

    infos.emplace(SBinaryOpKind::LessThanOrEqual, vector<BinOpInfo>{
        { intType, intType, boolType, RInternalBinaryOperator::LessThanOrEqual_Int_Int_Bool },
        { stringType, stringType, boolType, RInternalBinaryOperator::LessThanOrEqual_String_String_Bool }
    });

    infos.emplace(SBinaryOpKind::GreaterThanOrEqual, vector<BinOpInfo>{
        { intType, intType, boolType, RInternalBinaryOperator::GreaterThanOrEqual_Int_Int_Bool },
        { stringType, stringType, boolType, RInternalBinaryOperator::GreaterThanOrEqual_String_String_Bool }
    });

    infos.emplace(SBinaryOpKind::Equal, vector<BinOpInfo>{
        { intType, intType, boolType, RInternalBinaryOperator::Equal_Int_Int_Bool },
        { boolType, boolType, boolType, RInternalBinaryOperator::Equal_Bool_Bool_Bool },
        { stringType, stringType, boolType, RInternalBinaryOperator::Equal_String_String_Bool }
    });
}

const std::vector<BinOpInfo>& BinOpQueryService::GetInfos(SBinaryOpKind kind)
{
    static std::vector<BinOpInfo> empty;

    auto i = infos.find(kind);
    if (i != infos.end())
        return i->second;

    return empty;
}

} // Citron::SyntaxIR0Translator