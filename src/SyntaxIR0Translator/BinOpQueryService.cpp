#include "BinOpQueryService.h"

#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"

#include "MIR/MExp.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

BinOpInfo::BinOpInfo(RType* operandType0, RType* operandType1, RType* resultType, MInternalBinaryOperator rOperator)
    : operandType0(operandType0), operandType1(operandType1), resultType(resultType), rOperator(rOperator)
{
}

BinOpQueryService::BinOpQueryService(RFactory& factory)
{   
    auto intType = factory.MakeIntType();
    auto boolType = factory.MakeBoolType();
    auto stringType = factory.MakeStringType();

    infos.emplace(SBinaryOpKind::Multiply, vector<BinOpInfo>{ {intType, intType, intType, MInternalBinaryOperator::Multiply_Int_Int_Int } });
    infos.emplace(SBinaryOpKind::Divide, vector<BinOpInfo>{ {intType, intType, intType, MInternalBinaryOperator::Divide_Int_Int_Int } });
    infos.emplace(SBinaryOpKind::Modulo, vector<BinOpInfo>{ {intType, intType, intType, MInternalBinaryOperator::Modulo_Int_Int_Int}});
    infos.emplace(SBinaryOpKind::Add, vector<BinOpInfo>{
        { intType, intType, intType, MInternalBinaryOperator::Add_Int_Int_Int},
        { stringType, stringType, stringType, MInternalBinaryOperator::Add_String_String_String },
    });

    infos.emplace(SBinaryOpKind::Subtract, vector<BinOpInfo>{ { intType, intType, intType, MInternalBinaryOperator::Subtract_Int_Int_Int } });

    infos.emplace(SBinaryOpKind::LessThan, vector<BinOpInfo>{
        { intType, intType, boolType, MInternalBinaryOperator::LessThan_Int_Int_Bool },
        { stringType, stringType, boolType, MInternalBinaryOperator::LessThan_String_String_Bool }
    });

    infos.emplace(SBinaryOpKind::GreaterThan, vector<BinOpInfo>{
        { intType, intType, boolType, MInternalBinaryOperator::GreaterThan_Int_Int_Bool },
        { stringType, stringType, boolType, MInternalBinaryOperator::GreaterThan_String_String_Bool }
    });

    infos.emplace(SBinaryOpKind::LessThanOrEqual, vector<BinOpInfo>{
        { intType, intType, boolType, MInternalBinaryOperator::LessThanOrEqual_Int_Int_Bool },
        { stringType, stringType, boolType, MInternalBinaryOperator::LessThanOrEqual_String_String_Bool }
    });

    infos.emplace(SBinaryOpKind::GreaterThanOrEqual, vector<BinOpInfo>{
        { intType, intType, boolType, MInternalBinaryOperator::GreaterThanOrEqual_Int_Int_Bool },
        { stringType, stringType, boolType, MInternalBinaryOperator::GreaterThanOrEqual_String_String_Bool }
    });

    infos.emplace(SBinaryOpKind::Equal, vector<BinOpInfo>{
        { intType, intType, boolType, MInternalBinaryOperator::Equal_Int_Int_Bool },
        { boolType, boolType, boolType, MInternalBinaryOperator::Equal_Bool_Bool_Bool },
        { stringType, stringType, boolType, MInternalBinaryOperator::Equal_String_String_Bool }
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