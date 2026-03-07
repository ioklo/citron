#include "MExp.h"

#include <cassert>

#include "Infra/Unreachable.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RTypeArguments.h"
#include "NSymbol/NLambdaDecl.h"

#include "MLoc.h"

using namespace std;

namespace Citron {

void MExp_Load::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_Store::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_Stmt::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_PtrRef::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_BoolLiteral::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_IntLiteral::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_CallIntrinsic::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_Call::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_NewStruct::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_NewEnumElem::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_Nullable::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_NullableNullLiteral::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_Cast::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_Lambda::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_InlineBlock::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_Is::Accept(MExpVisitor& visitor) { visitor.Visit(this); }
void MExp_As::Accept(MExpVisitor& visitor) { visitor.Visit(this); }

RType* GetType_CallIntrinsic(MExp_CallIntrinsic* exp, RFactory* rFactory)
{
    switch (exp->kind)
    {
    case MExp_CallIntrinsicKind::LogicalNot_Bool_Bool: return rFactory->MakeBoolType();
    case MExp_CallIntrinsicKind::UnaryMinus_Int_Int: return rFactory->MakeIntType();
    case MExp_CallIntrinsicKind::PrefixInc_Int_Int: return rFactory->MakeIntType();
    case MExp_CallIntrinsicKind::PrefixDec_Int_Int: return rFactory->MakeIntType();
    case MExp_CallIntrinsicKind::PostfixInc_Int_Int: return rFactory->MakeIntType();
    case MExp_CallIntrinsicKind::PostfixDec_Int_Int: return rFactory->MakeIntType();
    case MExp_CallIntrinsicKind::Multiply_Int_Int_Int: return rFactory->MakeIntType();
    case MExp_CallIntrinsicKind::Divide_Int_Int_Int: return rFactory->MakeIntType();
    case MExp_CallIntrinsicKind::Modulo_Int_Int_Int: return rFactory->MakeIntType();
    case MExp_CallIntrinsicKind::Add_Int_Int_Int: return rFactory->MakeIntType();
    case MExp_CallIntrinsicKind::Subtract_Int_Int_Int: return rFactory->MakeIntType();
    case MExp_CallIntrinsicKind::LessThan_Int_Int_Bool: return rFactory->MakeBoolType();
    case MExp_CallIntrinsicKind::LessThan_String_String_Bool: return rFactory->MakeBoolType();
    case MExp_CallIntrinsicKind::GreaterThan_Int_Int_Bool: return rFactory->MakeBoolType();
    case MExp_CallIntrinsicKind::GreaterThan_String_String_Bool: return rFactory->MakeBoolType();
    case MExp_CallIntrinsicKind::LessThanOrEqual_Int_Int_Bool: return rFactory->MakeBoolType();
    case MExp_CallIntrinsicKind::LessThanOrEqual_String_String_Bool: return rFactory->MakeBoolType();
    case MExp_CallIntrinsicKind::GreaterThanOrEqual_Int_Int_Bool: return rFactory->MakeBoolType();
    case MExp_CallIntrinsicKind::GreaterThanOrEqual_String_String_Bool: return rFactory->MakeBoolType();
    case MExp_CallIntrinsicKind::Equal_Int_Int_Bool: return rFactory->MakeBoolType();
    case MExp_CallIntrinsicKind::Equal_Bool_Bool_Bool: return rFactory->MakeBoolType();
    case MExp_CallIntrinsicKind::Equal_String_String_Bool: return rFactory->MakeBoolType();
    case MExp_CallIntrinsicKind::GetIterator_List_ListIterator: return rFactory->MakeListIteratorType(exp->typeArgs->Get(0));
    }

    unreachable();
}

RType* GetType(MExp* exp, RFactory* rFactory)
{
    struct GetTypeVisitor
    {
        using ResultType = RType*;
        RFactory* rFactory;

        ResultType Visit(MExp_Load* exp) { return GetType(exp->loc, rFactory); }
        ResultType Visit(MExp_Store* exp) { return GetType(exp->dest, rFactory); }
        ResultType Visit(MExp_Stmt* exp) { return GetType(exp->finalExp, rFactory); }
        ResultType Visit(MExp_PtrRef* exp) 
        { 
            auto* innerLocType = GetType(exp->innerLoc, rFactory);
            return rFactory->MakePtrType(innerLocType);
        }
        ResultType Visit(MExp_BoolLiteral* exp) { return rFactory->MakeBoolType(); }
        ResultType Visit(MExp_IntLiteral* exp) { return rFactory->MakeIntType(); }
        ResultType Visit(MExp_CallIntrinsic* exp) { return GetType_CallIntrinsic(exp, rFactory); }

        ResultType Visit(MExp_Call* exp) { return GetType(exp->callable); }
        ResultType Visit(MExp_NewStruct* exp) 
        { 
            auto structDecl = exp->ctor->GetStructDecl();
            return rFactory->MakeStructType(structDecl, exp->typeArgs);
        }

        ResultType Visit(MExp_NewEnumElem* exp) { return rFactory->MakeEnumElemType(exp->enumElemDecl, exp->typeArgs); }
        ResultType Visit(MExp_Nullable* exp) { return rFactory->MakeNullableType(GetType(exp->innerExp, rFactory)); }
        ResultType Visit(MExp_NullableNullLiteral* exp) { return rFactory->MakeNullableType(exp->innerType); }
        ResultType Visit(MExp_Cast* exp) { return exp->targetType; }
        ResultType Visit(MExp_Lambda* exp) { return rFactory->MakeLambdaType(exp->lambdaDecl, exp->typeArgs); }
        ResultType Visit(MExp_InlineBlock* exp) { return exp->returnType; }
        ResultType Visit(MExp_Is* exp) { return rFactory->MakeBoolType(); }
        ResultType Visit(MExp_As* exp) { return exp->type; }
    };

    return Accept(GetTypeVisitor{rFactory}, exp);
}

} // namespace Citron