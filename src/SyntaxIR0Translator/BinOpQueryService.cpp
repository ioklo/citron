#include "BinOpQueryService.h"

#include "IR0/RTypes.h"
#include "IR0/RTypeFactory.h"

#include "IR0/NExp.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

BinOpInfo::BinOpInfo(const RTypePtr& operandType0, const RTypePtr& operandType1, const RTypePtr& resultType, NInternalBinaryOperator rOperator)
    : operandType0(operandType0), operandType1(operandType1), resultType(resultType), rOperator(rOperator)
{
}

BinOpQueryService::BinOpQueryService(RTypeFactory& factory)
{   
    auto intType = factory.MakeIntType();
    auto boolType = factory.MakeBoolType();
    auto stringType = factory.MakeStringType();

    infos.emplace(SBinaryOpKind::Multiply, vector<BinOpInfo>{ {intType, intType, intType, NInternalBinaryOperator::Multiply_Int_Int_Int } });
    infos.emplace(SBinaryOpKind::Divide, vector<BinOpInfo>{ {intType, intType, intType, NInternalBinaryOperator::Divide_Int_Int_Int } });
    infos.emplace(SBinaryOpKind::Modulo, vector<BinOpInfo>{ {intType, intType, intType, NInternalBinaryOperator::Modulo_Int_Int_Int}});
    infos.emplace(SBinaryOpKind::Add, vector<BinOpInfo>{
        { intType, intType, intType, NInternalBinaryOperator::Add_Int_Int_Int},
        { stringType, stringType, stringType, NInternalBinaryOperator::Add_String_String_String },
    });

    infos.emplace(SBinaryOpKind::Subtract, vector<BinOpInfo>{ { intType, intType, intType, NInternalBinaryOperator::Subtract_Int_Int_Int } });

    infos.emplace(SBinaryOpKind::LessThan, vector<BinOpInfo>{
        { intType, intType, boolType, NInternalBinaryOperator::LessThan_Int_Int_Bool },
        { stringType, stringType, boolType, NInternalBinaryOperator::LessThan_String_String_Bool }
    });

    infos.emplace(SBinaryOpKind::GreaterThan, vector<BinOpInfo>{
        { intType, intType, boolType, NInternalBinaryOperator::GreaterThan_Int_Int_Bool },
        { stringType, stringType, boolType, NInternalBinaryOperator::GreaterThan_String_String_Bool }
    });

    infos.emplace(SBinaryOpKind::LessThanOrEqual, vector<BinOpInfo>{
        { intType, intType, boolType, NInternalBinaryOperator::LessThanOrEqual_Int_Int_Bool },
        { stringType, stringType, boolType, NInternalBinaryOperator::LessThanOrEqual_String_String_Bool }
    });

    infos.emplace(SBinaryOpKind::GreaterThanOrEqual, vector<BinOpInfo>{
        { intType, intType, boolType, NInternalBinaryOperator::GreaterThanOrEqual_Int_Int_Bool },
        { stringType, stringType, boolType, NInternalBinaryOperator::GreaterThanOrEqual_String_String_Bool }
    });

    infos.emplace(SBinaryOpKind::Equal, vector<BinOpInfo>{
        { intType, intType, boolType, NInternalBinaryOperator::Equal_Int_Int_Bool },
        { boolType, boolType, boolType, NInternalBinaryOperator::Equal_Bool_Bool_Bool },
        { stringType, stringType, boolType, NInternalBinaryOperator::Equal_String_String_Bool }
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