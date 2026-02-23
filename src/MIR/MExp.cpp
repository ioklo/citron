#include "MExp.h"

#include <cassert>

#include "Infra/Unreachable.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RClassCtorDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RGlobalFuncDecl.h"
#include "NSymbol/NLambdaDecl.h"

#include "MLoc.h"


using namespace std;

namespace Citron {

RType* MExp_BitwiseCopy::GetType()
{
    return loc->GetType();
}

RType* MExp_BitwiseAssign::GetType()
{
    return dest->GetType();
}

MExp_Stmt::MExp_Stmt(std::vector<MStmt*>&& stmts, MExp* finalExp)
    : stmts{move(stmts)}, finalExp{finalExp}
{
}

RType* MExp_Stmt::GetType()
{
    return finalExp->GetType();
}

MExp_Shared::MExp_Shared(MCreate&& innerCreate, const RFactoryPtr& rFactory)
    : innerCreate{move(innerCreate)}, rFactory{rFactory}
{
}

RType* MExp_Shared::GetType()
{
    auto* innerType = Citron::GetType(innerCreate);
    return rFactory->MakeBoxType(innerType);
}

MExp_StaticBoxRef::MExp_StaticBoxRef(MLoc* loc, const RFactoryPtr& rFactory)
    : loc{loc}, rFactory{rFactory}
{
}

RType* MExp_StaticBoxRef::GetType()
{
    return rFactory->MakeBoxType(loc->GetType());
}

MExp_ClassMemberBoxRef::MExp_ClassMemberBoxRef(MLoc* holder, RClassVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
    : holder(holder), decl(decl), typeArgs(typeArgs), rFactory{rFactory}
{
}

RType* MExp_ClassMemberBoxRef::GetType()
{
    auto* declType = decl->GetDeclType(*typeArgs);
    return rFactory->MakeBoxType(declType);
}

MExp_StructIndirectMemberBoxRef::MExp_StructIndirectMemberBoxRef(MLoc* holder, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
    : holder{holder}, decl{decl}, typeArgs{typeArgs}, rFactory{rFactory}
{
}

RType* MExp_StructIndirectMemberBoxRef::GetType()
{
    auto* declType = decl->GetDeclType(*typeArgs);
    return rFactory->MakeBoxType(declType);
}

MExp_StructMemberBoxRef::MExp_StructMemberBoxRef(MLoc* parent, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
    : parent{parent}, decl{decl}, typeArgs{typeArgs}, rFactory{rFactory}
{
}

RType* MExp_StructMemberBoxRef::GetType()
{
    auto* declType = decl->GetDeclType(*typeArgs);
    return rFactory->MakeBoxType(declType);
}

MExp_PtrRef::MExp_PtrRef(MLoc* innerLoc, const RFactoryPtr& rFactory)
    : innerLoc{innerLoc}, rFactory{rFactory}
{
}

RType* MExp_PtrRef::GetType()
{
    auto* innerLocType = innerLoc->GetType();
    return rFactory->MakePtrType(innerLocType);
}

MExp_CastBoxedLambdaToFunc::MExp_CastBoxedLambdaToFunc(MExp* exp, RType_Func* funcType)
    : exp(exp), funcType(funcType)
{
}

RType* MExp_CastBoxedLambdaToFunc::GetType()
{
    return funcType;
}

MExp_BoolLiteral::MExp_BoolLiteral(bool value, const RFactoryPtr& rFactory)
    : value{value}, rFactory{rFactory}
{
}

RType* MExp_BoolLiteral::GetType()
{
    return rFactory->MakeBoolType();
}

MExp_IntLiteral::MExp_IntLiteral(int value, const RFactoryPtr& rFactory)
    : value{value}, rFactory{rFactory}
{
}

RType* MExp_IntLiteral::GetType()
{
    return rFactory->MakeIntType();
}

MExp_StringElem_Text::MExp_StringElem_Text(const string& text)
    : text{text}
{
}

MExp_StringElem_Exp::MExp_StringElem_Exp(MExp* mExp)
    : mExp{mExp}
{

}

MExp_String::MExp_String(vector<MExp_StringElem>&& elements, const RFactoryPtr& rFactory)
    : elements{move(elements)}, rFactory{rFactory}
{
}

RType* MExp_String::GetType()
{
    return rFactory->MakeStringType();
}

MExp_List::MExp_List(vector<MExp*>&& elems, RType* itemType, const RFactoryPtr& rFactory)
    : elems{move(elems)}, itemType{itemType}, rFactory{rFactory}
{
}

RType* MExp_List::GetType()
{
    return rFactory->MakeListType(itemType);
}

MExp_ListIterator::MExp_ListIterator(MLoc* listLoc, RType* iteratorType)
    : listLoc(listLoc), iteratorType{iteratorType}
{
}

RType* MExp_ListIterator::GetType()
{
    return iteratorType;
}

MExp_CallInternalUnaryOperator::MExp_CallInternalUnaryOperator(MInternalUnaryOperator op, MExp* operand, const RFactoryPtr& rFactory)
    : op{op}, operand{operand}, rFactory{rFactory}
{
}

RType* MExp_CallInternalUnaryOperator::GetType()
{
    switch (op)
    {
    case MInternalUnaryOperator::LogicalNot_Bool_Bool: return rFactory->MakeBoolType();
    case MInternalUnaryOperator::UnaryMinus_Int_Int: return rFactory->MakeIntType();
    case MInternalUnaryOperator::ToString_Bool_String: return rFactory->MakeStringType();
    case MInternalUnaryOperator::ToString_Int_String: return rFactory->MakeStringType();
    }

    unreachable();
}

MExp_CallInternalUnaryAssignOperator::MExp_CallInternalUnaryAssignOperator(MInternalUnaryAssignOperator op, MLoc* operand, const RFactoryPtr& rFactory)
    : op{op}, operand{operand}, rFactory{rFactory}
{
}

RType* MExp_CallInternalUnaryAssignOperator::GetType()
{
    switch(op)
    {
    case MInternalUnaryAssignOperator::PrefixInc_Int_Int:
    case MInternalUnaryAssignOperator::PrefixDec_Int_Int:
    case MInternalUnaryAssignOperator::PostfixInc_Int_Int:
    case MInternalUnaryAssignOperator::PostfixDec_Int_Int:
        return rFactory->MakeIntType();
    }

    unreachable();
}

MExp_CallInternalBinaryOperator::MExp_CallInternalBinaryOperator(MInternalBinaryOperator op, MExp* operand0, MExp* operand1, const RFactoryPtr& rFactory)
    : op{op}, operand0{operand0}, operand1{operand1}, rFactory{rFactory}
{
}

RType* MExp_CallInternalBinaryOperator::GetType()
{
    switch(op)
    {
        case MInternalBinaryOperator::Multiply_Int_Int_Int:
        case MInternalBinaryOperator::Divide_Int_Int_Int:
        case MInternalBinaryOperator::Modulo_Int_Int_Int:
        case MInternalBinaryOperator::Add_Int_Int_Int:
            return rFactory->MakeIntType();

        case MInternalBinaryOperator::Add_String_String_String:
            return rFactory->MakeStringType();

        case MInternalBinaryOperator::Subtract_Int_Int_Int:
            return rFactory->MakeIntType();

        case MInternalBinaryOperator::LessThan_Int_Int_Bool:
        case MInternalBinaryOperator::LessThan_String_String_Bool:
        case MInternalBinaryOperator::GreaterThan_Int_Int_Bool:
        case MInternalBinaryOperator::GreaterThan_String_String_Bool:
        case MInternalBinaryOperator::LessThanOrEqual_Int_Int_Bool:
        case MInternalBinaryOperator::LessThanOrEqual_String_String_Bool:
        case MInternalBinaryOperator::GreaterThanOrEqual_Int_Int_Bool:
        case MInternalBinaryOperator::GreaterThanOrEqual_String_String_Bool:
        case MInternalBinaryOperator::Equal_Int_Int_Bool:
        case MInternalBinaryOperator::Equal_Bool_Bool_Bool:
        case MInternalBinaryOperator::Equal_String_String_Bool:
            return rFactory->MakeBoolType();
        default:
            unreachable();
    }
}

MExp_CallGlobalFunc::MExp_CallGlobalFunc(RGlobalFuncDecl* rFuncDecl, RTypeArguments* rTypeArgs, const vector<MArgument>& args)
    : rFuncDecl{rFuncDecl}, rTypeArgs{rTypeArgs}, args{args}
{
}

RType* MExp_CallGlobalFunc::GetType()
{
    return rFuncDecl->GetReturnType(*rTypeArgs);
}

MExp_NewClass::MExp_NewClass(RClassCtorDecl* ctorDecl, RTypeArguments* typeArgs, const vector<MArgument>& args, const RFactoryPtr& rFactory)
    : ctorDecl{ctorDecl}, typeArgs{typeArgs}, args{args}, rFactory{rFactory}
{
}

RType* MExp_NewClass::GetType()
{
    auto classDecl = ctorDecl->GetClassDecl();
    assert(classDecl);

    return rFactory->MakeClassType(classDecl, typeArgs);
}

/////////////////////////////////////

MExp_CallClassFunc::MExp_CallClassFunc(RClassFuncDecl* decl, RTypeArguments* typeArgs, MLoc* instance, vector<MArgument>&& args)
    : decl{decl}, typeArgs{typeArgs}, instance{instance}, args{move(args)}
{
}

RType* MExp_CallClassFunc::GetType()
{
    return decl->GetReturnType(*typeArgs);
}

MExp_CastClass::MExp_CastClass(MExp* src, RType* classType)
    : src(src), classType(classType)
{
}

RType* MExp_CastClass::GetType()
{
    return classType;
}

MExp_NewStruct::MExp_NewStruct(RStructCtorDecl* ctor, RTypeArguments* typeArgs, vector<MArgument>&& args, const RFactoryPtr& rFactory)
    : ctor{ctor}, typeArgs{typeArgs}, args{move(args)}, rFactory{rFactory}
{
}

RType* MExp_NewStruct::GetType()
{
    auto structDecl = ctor->GetStructDecl();
    return rFactory->MakeStructType(structDecl, typeArgs);
}

MExp_CallStructFunc::MExp_CallStructFunc(RStructFuncDecl* decl, RTypeArguments* typeArgs, MLoc* instance, vector<MArgument>&& args)
    : decl{decl}, typeArgs{typeArgs}, instance{instance}, args{move(args)}
{
}

RType* MExp_CallStructFunc::GetType()
{
    return decl->GetReturnType(*typeArgs);
}

MExp_NewEnumElem::MExp_NewEnumElem(REnumElemDecl* enumElemDecl, RTypeArguments* typeArgs, vector<MArgument>&& args, const RFactoryPtr& rFactory)
    : enumElemDecl{enumElemDecl}, typeArgs{typeArgs}, args{move(args)}, rFactory{rFactory}
{
}

RType* MExp_NewEnumElem::GetType()
{
    return rFactory->MakeEnumElemType(enumElemDecl, typeArgs);
}

MExp_CastEnumElemToEnum::MExp_CastEnumElemToEnum(MExp* src, RType* enumType)
    : src(src), enumType(enumType)
{
}

RType* MExp_CastEnumElemToEnum::GetType()
{
    return enumType;
}

MExp_NullableValueNullLiteral::MExp_NullableValueNullLiteral(RType* innerType, const RFactoryPtr& rFactory)
    : innerType{innerType}, rFactory{rFactory}
{
}

RType* MExp_NullableValueNullLiteral::GetType()
{
    return rFactory->MakeNullableValueType(innerType);
}

MExp_NullableRefNullLiteral::MExp_NullableRefNullLiteral(RType* innerType, const RFactoryPtr& rFactory)
    : innerType{innerType}, rFactory{rFactory}
{
}

RType* MExp_NullableRefNullLiteral::GetType()
{
    return rFactory->MakeNullableRefType(innerType);
}

MExp_NewNullable::MExp_NewNullable(MExp* innerExp, const RFactoryPtr& rFactory)
    : innerExp{innerExp}, rFactory{rFactory}
{
}

RType* MExp_NewNullable::GetType()
{
    return rFactory->MakeNullableValueType(innerExp->GetType());
}

MExp_Lambda::MExp_Lambda(NLambdaDecl* lambdaDecl, RTypeArguments* typeArgs, const vector<MArgument>& args, const RFactoryPtr& rFactory)
    : lambdaDecl{lambdaDecl}, typeArgs{typeArgs}, args{args}, rFactory{rFactory}
{
}

RType* MExp_Lambda::GetType()
{
    return rFactory->MakeLambdaType(lambdaDecl, typeArgs);
}

MExp_CallLambda::MExp_CallLambda(RLambdaDecl* lambdaDecl, RTypeArguments* typeArgs, MLoc* callable, const vector<MArgument>& args)
    : lambdaDecl(lambdaDecl), typeArgs(typeArgs), callable(callable), args(args)
{
}

RType* MExp_CallLambda::GetType()
{
    return lambdaDecl->GetReturnType(*typeArgs);
}

MExp_InlineBlock::MExp_InlineBlock(const vector<MStmt*>& stmts, RType* returnType)
    : stmts(stmts), returnType(returnType)
{
}

RType* MExp_InlineBlock::GetType()
{
    return returnType;
}

MExp_ClassIsClass::MExp_ClassIsClass(MExp* exp, RType* classType, const RFactoryPtr& rFactory)
    : exp{exp}, classType{classType}, rFactory{rFactory}
{
}

RType* MExp_ClassIsClass::GetType()
{
    return rFactory->MakeBoolType();
}

MExp_ClassAsClass::MExp_ClassAsClass(MExp* exp, RType* classType, const RFactoryPtr& rFactory)
    : exp{exp}, classType{classType}, rFactory{rFactory}
{
}

RType* MExp_ClassAsClass::GetType()
{    
    return rFactory->MakeNullableRefType(classType);
}

MExp_ClassIsInterface::MExp_ClassIsInterface(MExp* exp, RType* interfaceType, const RFactoryPtr& rFactory)
    : exp{exp}, interfaceType{interfaceType}, rFactory{rFactory}
{
}

RType* MExp_ClassIsInterface::GetType()
{
    return rFactory->MakeBoolType();
}

MExp_ClassAsInterface::MExp_ClassAsInterface(MExp* exp, RType* interfaceType, const RFactoryPtr& rFactory)
    : exp{exp}, interfaceType{interfaceType}, rFactory{rFactory}
{
}

RType* MExp_ClassAsInterface::GetType()
{
    return rFactory->MakeNullableRefType(interfaceType);
}

MExp_InterfaceIsClass::MExp_InterfaceIsClass(MExp* exp, RType* classType, const RFactoryPtr& rFactory)
    : exp{exp}, classType{classType}, rFactory{rFactory}
{
}

RType* MExp_InterfaceIsClass::GetType()
{
    return rFactory->MakeBoolType();
}

MExp_InterfaceAsClass::MExp_InterfaceAsClass(MExp* exp, RType* classType, const RFactoryPtr& rFactory)
    : exp{exp}, classType{classType}, rFactory{rFactory}
{
}

RType* MExp_InterfaceAsClass::GetType()
{
    return rFactory->MakeNullableRefType(classType);
}

MExp_InterfaceIsInterface::MExp_InterfaceIsInterface(MExp* exp, RType* interfaceType, const RFactoryPtr& rFactory)
    : exp{exp}, interfaceType{interfaceType}, rFactory{rFactory}
{
}

RType* MExp_InterfaceIsInterface::GetType()
{
    return rFactory->MakeBoolType();
}

MExp_InterfaceAsInterface::MExp_InterfaceAsInterface(MExp* exp, RType* interfaceType, const RFactoryPtr& rFactory)
    : exp{exp}, interfaceType{interfaceType}, rFactory{rFactory}
{
}

RType* MExp_InterfaceAsInterface::GetType()
{
    return rFactory->MakeNullableRefType(interfaceType);
}

MExp_EnumIsEnumElem::MExp_EnumIsEnumElem(MExp* exp, RType_EnumElem* enumElemType, const RFactoryPtr& rFactory)
    : exp{exp}, enumElemType{enumElemType}, rFactory{rFactory}
{
}

RType* MExp_EnumIsEnumElem::GetType()
{
    return rFactory->MakeBoolType();
}

MExp_EnumAsEnumElem::MExp_EnumAsEnumElem(MExp* exp, RType_EnumElem* enumElemType, const RFactoryPtr& rFactory)
    : exp{exp}, enumElemType{enumElemType}, rFactory{rFactory}
{
}

RType* MExp_EnumAsEnumElem::GetType()
{
    return rFactory->MakeNullableValueType(enumElemType);
}

} // namespace Citron