#include "BinOpQueryService.h"

#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "MIR/MExp.h"
#include "MIR/MInitExp.h"

using namespace std;

namespace Citron {

BinOpQueryService::BinOpQueryService(RFactory& factory)
{   
    auto* intType = factory.MakeIntType();
    auto* boolType = factory.MakeBoolType();
    auto* stringType = factory.MakeStringType();

    infos.emplace(SBinaryOpKind::Multiply, vector<BinOpInfo>{{intType, intType, intType, BinOp_Exp{MExp_CallIntrinsicKind::Multiply_Int_Int_Int}}});
    infos.emplace(SBinaryOpKind::Divide, vector<BinOpInfo>{{intType, intType, intType, BinOp_Exp{MExp_CallIntrinsicKind::Divide_Int_Int_Int}}});
    infos.emplace(SBinaryOpKind::Modulo, vector<BinOpInfo>{{intType, intType, intType, BinOp_Exp{MExp_CallIntrinsicKind::Modulo_Int_Int_Int}}});
    infos.emplace(SBinaryOpKind::Add, vector<BinOpInfo>{
        {intType, intType, intType, BinOp_Exp{MExp_CallIntrinsicKind::Add_Int_Int_Int}},
        {stringType, stringType, stringType, BinOp_InitExp{MInitExp_CallIntrinsicKind::Add_String_String_String}},
    });

    infos.emplace(SBinaryOpKind::Subtract, vector<BinOpInfo>{ { intType, intType, intType, BinOp_Exp{MExp_CallIntrinsicKind::Subtract_Int_Int_Int} } });

    infos.emplace(SBinaryOpKind::LessThan, vector<BinOpInfo>{
        {intType, intType, boolType, BinOp_Exp{MExp_CallIntrinsicKind::LessThan_Int_Int_Bool}},
        {stringType, stringType, boolType, BinOp_Exp{MExp_CallIntrinsicKind::LessThan_StringPtr_StringPtr_Bool }}
    });

        infos.emplace(SBinaryOpKind::GreaterThan, vector<BinOpInfo>{
            {intType, intType, boolType, BinOp_Exp{MExp_CallIntrinsicKind::GreaterThan_Int_Int_Bool}},
            {stringType, stringType, boolType, BinOp_Exp{MExp_CallIntrinsicKind::GreaterThan_StringPtr_StringPtr_Bool}}
    });

    infos.emplace(SBinaryOpKind::LessThanOrEqual, vector<BinOpInfo>{
        {intType, intType, boolType, BinOp_Exp{MExp_CallIntrinsicKind::LessThanOrEqual_Int_Int_Bool}},
        {stringType, stringType, boolType, BinOp_Exp{MExp_CallIntrinsicKind::LessThanOrEqual_StringPtr_StringPtr_Bool}}
    });

    infos.emplace(SBinaryOpKind::GreaterThanOrEqual, vector<BinOpInfo>{
        { intType, intType, boolType, BinOp_Exp{MExp_CallIntrinsicKind::GreaterThanOrEqual_Int_Int_Bool}},
        {stringType, stringType, boolType, BinOp_Exp{MExp_CallIntrinsicKind::GreaterThanOrEqual_StringPtr_StringPtr_Bool}}
        });

    infos.emplace(SBinaryOpKind::Equal, vector<BinOpInfo>{
        {intType, intType, boolType, BinOp_Exp{MExp_CallIntrinsicKind::Equal_Int_Int_Bool}},
        {boolType, boolType, boolType, BinOp_Exp{MExp_CallIntrinsicKind::Equal_Bool_Bool_Bool}},
        {stringType, stringType, boolType, BinOp_Exp{MExp_CallIntrinsicKind::Equal_StringPtr_StringPtr_Bool}}
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